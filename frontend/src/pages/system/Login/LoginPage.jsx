import React, { useState } from "react";
import { Layout, Form, Input, Button, Divider } from "antd";
import "./login.less";
import { LoginAccount } from "./services/account";
import { token } from "~/utils/token";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { uuid } from "~/utils/utils";
import { useHistory } from "react-router-dom";
import { useGlobal } from "~/hooks";

export const LoginPage = (props) => {
  const [loading, setLoading] = useState(false);
  const [uniqueId, setUniqueId] = useState(uuid());
  const history = useHistory();

  const { siteTitle } = useGlobal();

  const handleSubmit = (values) => {
    let { username, password } = values;
    LoginAccount(
      { username, password, uuid: uniqueId },
      (data) => {
        token.save(data);
        history.push("/index");
      },
      (err) => {
        setLoading(false);
        setUniqueId(uuid());
      }
    );
    setLoading(true);
  };

  return (
    <Layout className="login animated fadeIn">
      <div className="model">
        <div className="login-form">
          <h3>{siteTitle}</h3>
          <Divider />
          <Form onFinish={handleSubmit}>
            <Form.Item
              name="username"
              rules={[{ required: true, message: "请输入用户名" }]}
            >
              <Input
                prefix={
                  <FontAwesomeIcon
                    icon="user"
                    fixedWidth
                    style={{ color: "rgba(0,0,0,.25)" }}
                  />
                }
                placeholder="用户名"
              />
            </Form.Item>
            <Form.Item
              name="password"
              rules={[{ required: true, message: "请输入密码" }]}
            >
              <Input
                prefix={
                  <FontAwesomeIcon
                    icon="lock"
                    fixedWidth
                    style={{ color: "rgba(0,0,0,.25)" }}
                  />
                }
                type="password"
                placeholder="密码"
                autoComplete="on"
              />
            </Form.Item>
            <Form.Item>
              <Button
                type="primary"
                htmlType="submit"
                className="login-form-button"
                loading={loading}
              >
                登录
              </Button>
            </Form.Item>
          </Form>
        </div>
      </div>
    </Layout>
  );
};
