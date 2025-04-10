import React, { useState, useReducer, useEffect } from "react";
import { useHistory, matchPath, Route, useLocation } from "react-router-dom";
import { Layout, BackTop, message, ConfigProvider } from "antd";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import zhCN from "antd/lib/locale/zh_CN";
import { AppHeader } from "./AppHeader";
import { AppAside } from "./AppAside";
import { token } from "../utils/token";
import { COMPONENTS, paths } from "../routes";
import { GetAccountMenu, ClearAccountCache } from "./services/account";
import { TabLayoutProvider, TabContainer } from "~/hooks";

const MENU_TOGGLE = "menuToggle";

const reducer = (state, action) => {
  switch (action.type) {
    case MENU_TOGGLE:
      return { ...state, menuToggle: !state.menuToggle };
    default:
      return state;
  }
};

const customizeRenderEmpty = () => (
  <div className="empty-data">
    <FontAwesomeIcon icon="info-square" />
    <p>暂无数据</p>
  </div>
);

const generateComponent = (key, title, prev, closable) => {
  const node = paths.find((v) => matchPath(key, v));
  return {
    title: title || node.title,
    key: key,
    path: node.path,
    prev: prev,
    closable: closable !== false,
    content: COMPONENTS[node.component],
  };
};

export const DefaultLayout = (props) => {
  const history = useHistory();
  const location = useLocation();
  const [menu, setMenu] = useState([]);
  useEffect(() => {
    GetAccountMenu(setMenu);
  }, []);

  const [state, dispatch] = useReducer(reducer, {
    menuToggle: localStorage.getItem("menuToggle") === "true",
  });

  useEffect(() => {
    if (!paths.find((v) => matchPath(location.pathname, v))) {
      history.push("/404");
    }
  }, [location.pathname]);

  const toggleClick = () => {
    localStorage.setItem("menuToggle", !state.menuToggle);
    dispatch({ type: "menuToggle" });
  };

  const loginOut = () => {
    token.remove();
    ClearAccountCache();
    history.push("/login");
    message.success("注销成功!");
  };

  const noSider = !menu || menu.length == 1;
  return (
    <Layout className="app">
      <TabLayoutProvider
        componentFactory={generateComponent}
        itemRender={(item) => (
          <Route
            path={item.path}
            exact={item.exact}
            children={(props) => <item.content {...props} />}
          />
        )}
      >
        <AppHeader
          hideToggle={noSider}
          menuToggle={state.menuToggle}
          menuClick={toggleClick}
          loginOut={loginOut}
        />
        <BackTop />
        <Layout>
          <ConfigProvider locale={zhCN} renderEmpty={customizeRenderEmpty}>
            {!noSider && <AppAside menuToggle={state.menuToggle} menu={menu} />}
            <Layout.Content className="content">
              <TabContainer noSider={noSider} />
            </Layout.Content>
          </ConfigProvider>
        </Layout>
      </TabLayoutProvider>
    </Layout>
  );
};
