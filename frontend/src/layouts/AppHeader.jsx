import React, { useMemo } from "react";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { Menu, Dropdown, Layout, Avatar, Spin } from "antd";

import { useAuth, useTabLayout, useGlobal } from "~/hooks";
import { useHistory } from "react-router-dom";
import { icon } from "@fortawesome/fontawesome-svg-core";

const { Header } = Layout;

export const AppHeader = (props) => {
  let { menuClick, menuToggle, loginOut, hideToggle } = props;
  const { user } = useAuth();
  const { config } = useGlobal();
  const { addTab } = useTabLayout();
  const history = useHistory();

  const items = useMemo(
    () => [
      {
        type: "group",

        label: (
          <>
            <span>用户设置</span>
            <span style={{ float: "right" }}>{user && user.userName}</span>
          </>
        ),
      },
      {
        type: "divider",
      },
      {
        key: "profile",
        icon: <FontAwesomeIcon icon="edit" />,
        onClick: () => {
          addTab({ key: "/account/profile", title: "个人设置" });
          history.push("/account/profile");
        },
        label: "个人设置",
      },
      {
        key: "logout",
        onClick: loginOut,
        icon: <FontAwesomeIcon icon="sign-out" />,
        label: "退出登录",
      },
    ],
    []
  );

  return (
    <Header>
      <div className={"left" + (menuToggle ? " fold" : "")}>
        {!hideToggle && (
          <FontAwesomeIcon
            onClick={menuClick}
            icon={menuToggle ? "bars" : "angle-double-left"}
          />
        )}
        <h1>{config.siteTitle}</h1>
      </div>
      <div className="right">
        <div>
          <Dropdown menu={{ items }} overlayStyle={{ width: 160 }}>
            <div className="ant-dropdown-link">
              {user && user.profiles ? (
                <>
                  <span className="name">{`${
                    user.profiles.fullName || user.userName
                  }`}</span>
                  <Avatar
                    icon={<FontAwesomeIcon icon="user" />}
                    src={user.profiles.avatar}
                    alt="avatar"
                    style={{ cursor: "pointer" }}
                  />
                </>
              ) : (
                <span className="name">...</span>
              )}
            </div>
          </Dropdown>
        </div>
      </div>
    </Header>
  );
};
