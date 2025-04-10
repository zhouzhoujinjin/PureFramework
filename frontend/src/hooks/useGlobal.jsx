import React, { useState, useEffect, useContext, createContext } from "react";
import request from "../utils/request";
import { API } from "../config";
import { useHistory } from "react-router-dom";
import { ConfigProvider } from "antd";
import zhCN from "antd/es/locale/zh_CN";
import dayjs from "dayjs";

const GetConfig = async (action) => {
  const data = await request.get(`${API}/config`);
  action(data);
};

const GlobalContext = createContext();

export const GlobalProvider = (props) => {
  const history = useHistory();
  const [updated, setUpdated] = useState(false);
  const [state, setState] = useState({
    config: {},
    refresh: () => setUpdated(!updated),
  });
  useEffect(() => {
    GetConfig((config) => {
      if (config.siteUnderMaintenance === true) {
        history.push("/maintenance");
      }
      setState((s) => ({
        ...s,
        config,
      }));
    });
  }, [updated]);

  return (
    <GlobalContext.Provider value={state}>
      <ConfigProvider locale={zhCN}>{props.children}</ConfigProvider>
    </GlobalContext.Provider>
  );
};

export const useGlobal = () => {
  const data = useContext(GlobalContext);
  return { ...data };
};
