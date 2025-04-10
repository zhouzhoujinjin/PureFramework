import React, { useState, useEffect, useCallback } from "react";
import { Button, Table } from "antd";
import { PageWrapper } from "~/components/PageWrapper";
import { useLocation, useHistory } from "react-router-dom";
import { GetValueSpaces } from "./services/valueSpace";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { useTabLayout } from "~/hooks/useTabLayout";
import { useDocumentTitle } from "~/hooks/useDocumentTitle";

export const ValueSpacesPage = () => {
  const [loading, setLoading] = useState(false);
  const [pagination, setPagination] = useState({});
  const [currentPage, setCurrentPage] = useState(1);
  const [list, setList] = useState([]);
  const history =  useHistory();
  const { addTab, refreshSwitches } = useTabLayout();
  const refreshSwitch = refreshSwitches["/valueSpaces"];
  useDocumentTitle("/valueSpaces", "值空间管理");
  const columns = [
    {
      title: "名称",
      dataIndex: "name",
      width: "15%",
    },
    {
      title: "标题",
      dataIndex: "title",
      width: "15%",
    },
    {
      title: "可编辑",
      dataIndex: "configureLevel",
      width: "10%",
      render: (val) => (val === "System" ? "系统" : "可配置"),
    },
    {
      title: "内容",
      dataIndex: "conditions",
      render: (val) => {
        if (typeof val === "string") {
          return val;
        }
        const items = Object.keys(val)
          .sort()
          .slice(0, 5)
          .map((x, i) => (
            <span key={x}>
              {i > 0 && "；"}
              {x}: {val[x]}
            </span>
          ));
        if (Object.keys(val).length > 5) {
          items.push("......");
        }
        return <>{items}</>;
      },
    },
    {
      title: "操作",
      render: (text, record) => {
        return (
          <>
            {record.configureLevel === "Configurable" ? (
              <Button
                onClick={() => {
                  addTab({
                    key: `/system/valueSpaces/${record.name}`,
                    title: `值空间：${record.title}`,
                    prev: "/system/valueSpaces",
                  });
                  history.push(`/system/valueSpaces/${record.name}`);
                }}
              >
                <FontAwesomeIcon icon="pencil" />
                编辑
              </Button>
            ) : null}
          </>
        );
      },
      width: 110,
    },
  ];

  const handleTableChange = (pagination) => {
    setCurrentPage(pagination.current);
  };

  const refreshTable = useCallback(() => {
    setLoading(true);
    GetValueSpaces({ page: currentPage }, (result) => {
      setList(result.data);
      setPagination({
        total: result.total,
      });
      setLoading(false);
    });
  }, [currentPage]);

  useEffect(() => {
    refreshTable();
  }, [currentPage, refreshSwitch, refreshTable]);

  return (
    <PageWrapper
      title="值空间管理"
      majorAction={
        <Button
          onClick={() => {
            addTab({
              key: "/system/valueSpaces/create",
              title: `添加值空间`,
              prev: "/system/valueSpaces",
            });
            history.push(`/system/valueSpaces/create`);
          }}
        >
          <FontAwesomeIcon icon="plus" fixedWidth />
          新建
        </Button>
      }
    >
      <Table
        loading={loading}
        dataSource={list || []}
        columns={columns}
        rowKey="name"
        pagination={{ ...pagination, current: currentPage } || {}}
        onChange={handleTableChange}
      />
    </PageWrapper>
  );
};
