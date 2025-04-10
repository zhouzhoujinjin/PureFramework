import React from "react";
import { Result, Button } from "antd";
import { useHistory } from "react-router-dom";

export const View404 = () => {
  const history =  useHistory();
  return (
    <Result
      status="404"
      title="404"
      subTitle="找不到网页"
      extra={
        <Button type="primary" onClick={() => history.push("/")}>
          回到首页
        </Button>
      }
    />
  );
};

