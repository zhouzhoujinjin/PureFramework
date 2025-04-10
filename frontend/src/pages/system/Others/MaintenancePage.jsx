import React from "react";
import { Result, Button } from "antd";
import { useHistory } from "react-router-dom";

export const MaintenancePage = () => {
  const history =  useHistory();
  return (
    <Result
      status="500"
      title="维护中"
      subTitle="请耐心等待"
      extra={
        <Button
          type="primary"
          onClick={() => history.push("mailto:admin@cyber-stone.com")}
        >
          联系管理员
        </Button>
      }
    />
  );
};
