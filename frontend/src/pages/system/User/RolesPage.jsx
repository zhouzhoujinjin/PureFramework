import React, { useState, useEffect, useCallback } from "react";
import { Table, Button, Divider, Popconfirm } from "antd";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { GetRoles, DeleteRole } from "./services/role";
import { useHistory } from "react-router-dom";
import { PageWrapper } from "~/components/PageWrapper";
import { useTabLayout } from "~/hooks/useTabLayout";
import { useDocumentTitle } from "~/hooks/useDocumentTitle";
import { getFriendlyUserName } from "~/utils/utils";

export const RolesPage = (props) => {
  const [loading, setLoading] = useState(false);
  const [pagination, setPagination] = useState({});
  const [currentPage, setCurrentPage] = useState(1);
  const [list, setList] = useState([]);
  const history =  useHistory();
  const { addTab, refreshSwitches } = useTabLayout();
  const refreshSwitch = refreshSwitches["/roles"];
  useDocumentTitle("/roles", "角色管理");
  const columns = [
    {
      title: "名称",
      dataIndex: "name",
      width: "10%",
    },
    {
      title: "标题",
      dataIndex: "title",
      width: "15%",
    },
    {
      title: "用户",
      dataIndex: "users",
      render: (val) =>
        val
          ? val
              .slice(0, 5)
              .map((x) => getFriendlyUserName(x))
              .join(", ") + (val.length > 5 ? "..." : "")
          : "",
    },
    {
      title: "操作",
      align: "right",
      render: (text, record) => (
        <>
          <Button
            onClick={() => {
              addTab({
                key: `/roles/${record.name}`,
                title: `角色：${record.title}`,
                prev: "/roles",
              });
              history.push(`/roles/${record.name}`);
            }}
          >
            <FontAwesomeIcon icon="pencil" />
            编辑
          </Button>
          <Divider type="vertical" />
          <Popconfirm
            title="确定删除此角色吗？"
            icon={<FontAwesomeIcon icon="question-circle" color="red" />}
            onConfirm={() => handleDeleteConfirmed(record.name)}
          >
            <Button type="danger">
              <FontAwesomeIcon icon="trash" /> 删除
            </Button>
          </Popconfirm>
        </>
      ),
      width: 209,
    },
  ];

  const handleTableChange = (pagination) => {
    setCurrentPage(pagination.current);
  };

  const handleCreateRole = () => {
    addTab({
      key: `/roles/create`,
      title: "添加角色",
      prev: "/roles",
    });
    history.push("/roles/create");
  };

  const handleDeleteConfirmed = (name) => {
    DeleteRole(name, (data) => {
      if (data) {
        setList(list.filter((o) => o.name !== name));
      }
    });
  };

  const refreshTable = useCallback(() => {
    setLoading(true);
    GetRoles({ page: currentPage }, (result) => {
      setList(result.data);
      setPagination({
        total: result.totalPage,
      });
      setLoading(false);
    });
  }, [currentPage]);

  useEffect(() => {
    refreshTable();
  }, [currentPage, refreshSwitch, refreshTable]);

  return (
    <PageWrapper
      title="角色列表"
      majorAction={
        <Button type="success" onClick={handleCreateRole}>
          <FontAwesomeIcon icon="plus" /> 添加角色
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
