import React, { useState, useEffect } from "react";
import { Menu } from "antd";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { paths } from "~/routes";
import { Link, useLocation, withRouter, matchPath } from "react-router-dom";

// 处理 pathname
const getOpenKeys = (path, collapsed) => {
  const newArr = [];
  let item = menuMap[path];
  while (item) {
    newArr.push(item.path);
    item = item.parent;
  }
  return collapsed ? [] : newArr;
};

let menuMap = {};
const flatMenuData = (children, parent) => {
  children.forEach((m) => {
    menuMap[m.path] = m;
    m.parent = parent;
    if (m.children && m.children.length > 0) {
      flatMenuData(m.children, m);
    }
  });
};

const internalSideMenu = (props) => {
  const [state, setState] = useState({
    openKeys: [],
    selectedKeys: [],
  });

  const { pathname } = useLocation();

  const { menu: menuData } = props;

  useEffect(() => {
    flatMenuData(menuData);
  }, [menuData]);

  let { openKeys, selectedKeys } = state;
  let menuProps = props.collapsed ? {} : { openKeys };

  // 页面刷新的时候可以定位到 menu 显示
  useEffect(() => {
    let { openKeys, selectedKeys } = props;
    if (openKeys && selectedKeys) {
      setState((prevState) => ({
        ...prevState,
        openKeys,
        selectedKeys,
      }));
    } else {
      setState((prevState) => {
        const router = paths.find((v) => matchPath(pathname, v.path));
        const keys = router ? getOpenKeys(router.path) : [];
        return {
          ...prevState,
          selectedKeys: keys,
          openKeys: keys,
        };
      });
    }
  }, [pathname, props]);

  // 只展开一个 SubMenu
  const onOpenChange = (openKeys) => {
    setState((prevState) => {
      if (openKeys.length === 0 || openKeys.length === 1) {
        return { ...prevState, openKeys };
      }
      const latestOpenKey = openKeys[openKeys.length - 1];

      // 这里与定义的路由规则有关
      if (latestOpenKey.includes(openKeys[0])) {
        return { ...prevState, openKeys };
      } else {
        return { ...prevState, openKeys: [latestOpenKey] };
      }
    });
  };

  const renderMenuItem = ({ path, iconName, title }) => ({
    key: path,
    icon: iconName && <FontAwesomeIcon icon={iconName} fixedWidth />,
    label: (
      <Link to={path}>
        <span className="title">{title}</span>
      </Link>
    ),
  });

  // 循环遍历数组中的子项 children ，生成子级 menu
  const renderSubMenu = ({
    path,
    iconName,
    title,
    children,
    hideChildren,
  }) => ({
    key: path,
    icon: iconName && <FontAwesomeIcon icon={iconName} fixedWidth />,
    label: <span className="title">{title}</span>,
    children:
      !hideChildren &&
      children &&
      children.map((item) => {
        return !item.hideChildren && item.children && item.children.length > 0
          ? renderSubMenu(item)
          : renderMenuItem(item);
      }),
  });

  return (
    <Menu
      mode="inline"
      {...menuProps}
      selectedKeys={selectedKeys}
      onClick={({ key }) => {
        setState((prevState) => ({ ...prevState, selectedKeys: [key] }));
        props.onClick && props.onClick(key);
      }}
      onOpenChange={onOpenChange}
      items={
        props.menu &&
        props.menu.map((item) => {
          return !item.hideChildren && item.children && item.children.length > 0
            ? renderSubMenu(item)
            : renderMenuItem(item);
        })
      }
    ></Menu>
  );
};

export const SideMenu = withRouter(internalSideMenu);
