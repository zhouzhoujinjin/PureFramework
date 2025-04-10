import React from "react";
import { Card } from "antd";
import "./PageWrapper.less";

export const PageWrapper = ({
  cardHeight,
  sidebar,
  children,
  title,
  majorAction,
  extras,
  showDefaultCard = true,
  style
}) => {
  const cardStyle = cardHeight ? { minHeight: cardHeight } : {};
  return (
    <>
      {sidebar && <div className="secondary-sidebar">{sidebar}</div>}
      <div className="page-wrapper" style={style}>
        <div className="page-wrapper-header">
          <h2>{title}</h2>
          {majorAction && <div className="major-action">{majorAction}</div>}
          {extras && <div className="extras">{extras}</div>}
        </div>
        {showDefaultCard ? <Card style={cardStyle}>{children}</Card> : children}
      </div>
    </>
  );
};
