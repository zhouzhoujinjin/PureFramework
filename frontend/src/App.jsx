import React, { Suspense } from "react";
import { Switch, Route, Redirect, BrowserRouter } from "react-router-dom";
import "animate.css";
import "./styles/App.less";
import { message } from "antd";
import { DefaultLayout } from "./layouts/DefaultLayout";
import { GlobalProvider, AuthProvider } from "~/hooks";
import { MaintenancePage, View404, View403, View500, LoginPage } from "./pages";



message.config({
  maxCount: 1,
});

const App = () => {
  return (
    <Suspense fallback={<div>Loading... </div>}>
      <BrowserRouter basename="/">
        <GlobalProvider>
          <Switch>
            <Route path="/" exact render={() => <Redirect to="/index" />} />
            <Route path="/login" component={LoginPage} />            
            <Route path="/maintenance" component={MaintenancePage} />
            <Route path="/500" component={View500} />
            <Route path="/404" component={View404} />
            <Route path="/403" component={View403} />
            <AuthProvider signInRoute="/login">
              <Route component={DefaultLayout} />
            </AuthProvider>
          </Switch>
        </GlobalProvider>
      </BrowserRouter>
    </Suspense>
  );
};

export default App;
