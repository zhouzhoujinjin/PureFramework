import React from "react";
import { Layout } from "antd";
import { SideMenu } from "~/components";
import { AppFooter } from "./AppFooter";
import { useTabLayout } from "~/hooks";

const { Sider } = Layout;

export const AppAside = (props) => {
  let { menuToggle, menu } = props;
  const { addTab } = useTabLayout();
  const onClick = (key) => {
    addTab({ key });
  };
  return (
    <Sider
      theme="light"
      className="aside"
      trigger={null}
      collapsible
      collapsed={menuToggle}
    >
      <SideMenu
        menu={menu}
        collapsed={menuToggle}
        onClick={(key) => onClick(key)}
      ></SideMenu>
      <AppFooter />
    </Sider>
  );
};
