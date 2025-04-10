import React, { Suspense } from "react";
import { createRoot } from "react-dom/client";
import { BrowserRouter } from "react-router-dom";
import localeData from 'dayjs/plugin/localeData';
import weekday from 'dayjs/plugin/weekday';
import dayjs from 'dayjs';
import zhCN from 'dayjs/locale/zh-cn'

import App from "./App";
import "antd/dist/antd.min.css";

import { config } from "@fortawesome/fontawesome-svg-core";

const error = console.error;
console.error = (...args) => {
  if (/defaultProps/.test(args[0])) return;
  if (/Tabs.TabPane is deprecated/.test(args[0])) return;
  if (/findDOMNode/.test(args[0])) return;
  error(...args);
};

dayjs.extend(localeData).extend(weekday);
dayjs.locale(zhCN)

config.styleDefault = "light";
const root = createRoot(document.getElementById("root"));
root.render(
  <Suspense fallback={<div>Loading... </div>}>
    <BrowserRouter basename="/">
      <App />
    </BrowserRouter>
  </Suspense>
);
