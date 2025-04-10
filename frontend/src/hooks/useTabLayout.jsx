import React, { useState, useContext, useEffect, useCallback } from "react";
import { useHistory } from "react-router-dom";
import { Tabs, Tooltip } from "antd";
const { TabPane } = Tabs;

const TabLayoutContext = React.createContext();
const HOME_KEY = "/index";

const createComp = (factory, key, title, prev, closable) => {
  const node = factory(key, title, prev, closable);
  if (node.title && node.title.length > 6) {
    node.tip = node.title;
    node.title =
      node.title.substring(0, 2) +
      "..." +
      node.title.substring(node.title.length - 3, node.title.length);
  }
  return node;
};

export const useTabLayout = () => {
  const { currentTab, addTab, replaceTab, refreshTab, refreshSwitches } =
    useContext(TabLayoutContext);

  return { currentTab, addTab, replaceTab, refreshTab, refreshSwitches };
};

export const TabContainer = React.forwardRef((props, ref) => {
  const context = useContext(TabLayoutContext);
  const { currentTab, onChange, tabList, onEdit, itemRender } = context;
  return (
    <Tabs
      className={props.noSider && "no-sider"}
      size="smalls"
      activeKey={currentTab}
      onChange={onChange}
      tabPosition="top"
      hideAdd
      type="editable-card"
      onEdit={onEdit}
      ref={ref}
    >
      {tabList &&
        tabList.map((item) => {
          return (
            <TabPane
              tab={
                <div
                  onMouseUp={(e) =>
                    e.button === 1 && onEdit(item.key, "remove")
                  }
                >
                  <Tooltip
                    placement="bottom"
                    mouseEnterDelay={0}
                    title={item.tip}
                  >
                    {item.title}
                  </Tooltip>
                </div>
              }
              key={item.key}
              closable={item.closable}
              title={item.title}
            >
              {itemRender(item)}
            </TabPane>
          );
        })}
    </Tabs>
  );
});

export const TabLayoutProvider = ({
  componentFactory,
  children,
  itemRender,
}) => {
  const [tabList, setTabList] = useState([]);
  const history = useHistory();
  const [currentTab, setCurrentTab] = useState(HOME_KEY);
  const [refreshSwitches, setRefreshSwitches] = useState({});

  const storeCurrent = (key) => sessionStorage.setItem("ACTIVE_TAB", key);

  const setCurrent = useCallback((key) => {
    storeCurrent(key);
    setCurrentTab(key);
    return key;
  }, []);

  useEffect(() => {
    const openTabs = sessionStorage.getItem("OPENED_TABS");
    let curr =
      window.location.pathname !== ""
        ? window.location.pathname
        : sessionStorage.getItem("ACTIVE_TAB") || HOME_KEY;
    if (openTabs) {
      const keys = JSON.parse(openTabs);
      if (keys.findIndex((key) => key.key === curr) === -1) {
        curr = HOME_KEY;
      }
      const tabs = keys.map((x, index) =>
        createComp(componentFactory, x.key, x.title, x.prev, index !== 0)
      );
      setTabList(tabs);
      setCurrentTab(curr);
    } else {
      const tabs = [];
      if (curr !== HOME_KEY) {
        tabs.push(createComp(componentFactory, HOME_KEY, null, null, false));
      }
      tabs.push(
        createComp(componentFactory, curr, null, null, curr !== HOME_KEY)
      );
      setTabList(tabs);
      saveTabs(tabs);
    }
    setCurrent(curr);
  }, [setCurrent, componentFactory, window.location.pathname]);

  const removeTab = (key) => {
    const index = tabList.findIndex((v) => v.key === key);
    if (index > 0) {
      const node = tabList[index];
      let nextKey =
        (tabList.findIndex((v) => v.key === node.prev) >= 0 && node.prev) ||
        currentTab;

      if (currentTab === key && nextKey === currentTab) {
        // 关闭的是当前 tab，同时当前 tab 不是最后一个 tab
        if (index >= 0 && index < tabList.length - 1) {
          nextKey = tabList[index + 1].key;
        } else {
          nextKey = tabList[index - 1 >= 0 ? index - 1 : 0].key;
        }
      }

      tabList.splice(index, 1);
      delete refreshSwitches[key];
      setRefreshSwitches({ ...refreshSwitches });
      saveTabs([...tabList]);
      return setCurrent(nextKey);
    }
  };
  const onChange = (key) => {
    setCurrent(key);
    history.push(key);
  };

  const onEdit = (targetKey, action) => {
    if (action !== "remove") {
      return;
    }
    const key = removeTab(targetKey);
    history.push(key);
  };

  const saveTabs = (list) => {
    sessionStorage.setItem(
      "OPENED_TABS",
      JSON.stringify(
        list.map((t) => ({
          key: t.key,
          title: t.tip || t.title,
          prev: t.prev,
          closable: t.closable,
        }))
      )
    );
    setTabList(list);
  };

  const addTab = ({ key, title, prev, closable }) => {
    if (tabList.findIndex((v) => v.key === key) < 0) {
      const node = createComp(componentFactory, key, title, prev, closable);
      if (prev) {
        const index = tabList.findIndex((v) => v.key === prev) + 1;
        index === 0 ? tabList.push(node) : tabList.splice(index, 0, node);
      } else {
        tabList.push(node);
      }
      saveTabs([...tabList]);
    }
    setCurrent(key);
  };

  const replaceTab = ({ oldKey, title, newKey }) => {
    const index = tabList.findIndex((v) => v.key === oldKey);

    if (index > 0) {
      const oldNode = tabList[index];
      const tab = createComp(
        componentFactory,
        newKey,
        title,
        oldNode.prev,
        true
      );
      tabList.splice(index, 1, tab);
      saveTabs([...tabList]);
      return setCurrent(newKey);
    }
  };

  const refreshTab = (key) => {
    refreshSwitches[key] = !refreshSwitches[key];
    setRefreshSwitches({ ...refreshSwitches });
  };

  return (
    <TabLayoutContext.Provider
      value={{
        currentTab,
        addTab,
        replaceTab,
        refreshTab,
        itemRender,
        tabList,
        onChange,
        onEdit,
        refreshSwitches,
      }}
    >
      {children}
    </TabLayoutContext.Provider>
  );
};
