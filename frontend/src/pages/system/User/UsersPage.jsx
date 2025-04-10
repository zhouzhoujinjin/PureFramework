import React, { useState, useEffect, useCallback } from "react";
import { Link } from "react-router-dom";
import {
  Table,
  Tag,
  Button,
  Divider,
  Popconfirm,
  Input,
  message,
  Radio,
} from "antd";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { useHistory } from "react-router-dom";
import { PageWrapper } from "~/components/PageWrapper";
import { GetUsers, SetDelete, SetActive, ResetPassword } from "./services/user";
import { useTabLayout } from "~/hooks/useTabLayout";
import { useDocumentTitle } from "~/hooks/useDocumentTitle";

export const UsersPage = () => {
  const [loading, setLoading] = useState(false);
  const [pagination, setPagination] = useState({ size: 20 });
  const [currentPage, setCurrentPage] = useState(1);
  const [list, setList] = useState([]);
  const [filters, setFilters] = useState({});
  const history =  useHistory();
  const { addTab, refreshSwitches } = useTabLayout();
  const refreshSwitch = refreshSwitches["/users"];
  useDocumentTitle("/users", "用户管理");

  const columns = [
    {
      key: "username",
      title: "用户名",
      dataIndex: "userName",
      width: "15%",
    },
    {
      key: "fullName",
      title: "姓名",
      dataIndex: "fullName",
      width: "15%",
    },    
    {
      key: "mobile",
      title: "手机号",
      dataIndex: "mobile",
      width: "15%",
    },
    {
      key: "roles",
      title: "角色",
      dataIndex: "roles",
      render: (val) => {
        if (val) {
          return Object.keys(val).map((k, i) => {
            return (
              <Link
                key={k}
                onClick={() => {
                  addTab({
                    key: `/roles/${k}`,
                    title: `角色: ${val[k]}`,
                    prev: "/roles",
                    closable: true,
                  });
                  history.push(`/roles/${k}`);
                }}
                to="#"
              >
                {i > 0 && <span>,&nbsp;</span>}
                {val[k]}
              </Link>
            );
          });
        }
        return null;
      },
    },
    {
      key: "deleted",
      title: "状态",
      dataIndex: "deleted",
      render: (val) =>
        !val ? <Tag color="green">正常</Tag> : <Tag color="orange">停用</Tag>,
    },
    {
      title: "操作",
      key: "operation",
      align: "right",
      render: (text, record) => (
        <>
          <Button
            onClick={() => {
              addTab({
                key: `/users/${record.userName}`,
                title: `用户: ${record.fullName || record.useName}`,
                prev: "/users",
              });
              history.push(`/users/${record.userName}`);
            }}
          >
            <FontAwesomeIcon icon="pencil" />
            编辑
          </Button>
          <Button
            className="ant-btn-warning"
            onClick={() => onResetPassword(record.userName)}
          >
            <FontAwesomeIcon icon="redo" />
            重置密码
          </Button>
          <Divider type="vertical" />
          {!record.deleted ? (
            <Popconfirm
              title="用户无法通过此账号登录系统，您确认操作吗？"
              onConfirm={() => onDeleteConfirmed(record.userName)}
            >
              <Button type="danger">
                <FontAwesomeIcon icon="trash" />
                停用
              </Button>
            </Popconfirm>
          ) : (
            <Button
              className="ant-btn-success"
              onClick={() => onActive(record.userName)}
            >
              <FontAwesomeIcon icon="unlock" />
              激活
            </Button>
          )}
        </>
      ),
      width: 330,
    },
  ];

  const handleTableChange = (pagination) => setCurrentPage(pagination.current);

  const handleCreateUser = () => {
    addTab({
      key: `/users/create`,
      title: "添加用户",
      prev: "/users",
    });
    history.push("/users/create");
  };

  const handleStatusChange = (e) => {
    setFilters(f => ({...f, status:e.target.value}));
    setCurrentPage(1);
  };
  const handleSearchChange = (v) => {
    setFilters(f => ({...f, query:v}));
    setCurrentPage(1);
  };

  const onDeleteConfirmed = (userName) => {
    SetDelete(userName, (data) => {
      if (data) {
        message.success("操作成功");
        setList(list.filter((u) => u.userName !== userName));
      } else {
        message.error("操作失败");
      }
    });
  };

  const onActive = (userName) => {
    SetActive(userName, (data) => {
      if (data) {
        message.success("操作成功");
        setList(list.filter((u) => u.userName !== userName));
      } else {
        message.error("操作失败");
      }
    });
  };

  const onResetPassword = (userName) => {
    ResetPassword(userName, (data) => {
      data ? message.success("操作成功") : message.error("操作失败");
    });
  };

  const refreshTable = useCallback(() => {
    setLoading(true);
    GetUsers({ page: currentPage, size: 20 }, filters, (result) => {
      setList(result.data);
      setPagination({
        pageSize: 20,
        total: result.total,
      });
      setLoading(false);
    });
  }, [currentPage, filters]);

  useEffect(() => {
    refreshTable();
  }, [currentPage, refreshSwitch, refreshTable]);

  return (
    <PageWrapper
      title={`${filters.status === "deleted" ? "已删除" : ""}${
        filters.status === "invisible" ? "已隐藏" : ""
      }用户列表`}
      majorAction={
        <Button onClick={handleCreateUser}>
          <FontAwesomeIcon icon="plus" /> 添加用户
        </Button>
      }
      extras={
        <>
          <Input.Search style={{ width: 200 }} onSearch={handleSearchChange} />
          <Radio.Group value={filters.status} onChange={handleStatusChange}>
            <Radio.Button value="">
              <FontAwesomeIcon icon="user" fixedWidth />
              正常用户
            </Radio.Button>
            <Radio.Button value="invisible">
              <FontAwesomeIcon icon="eye-slash" fixedWidth />
              已隐藏用户
            </Radio.Button>
            <Radio.Button value="deleted">
              <FontAwesomeIcon icon="trash" fixedWidth />
              已删除用户
            </Radio.Button>
          </Radio.Group>
        </>
      }
    >
      <Table
        loading={loading}
        dataSource={list || []}
        columns={columns}
        rowKey="userName"
        pagination={{ ...pagination, current: currentPage } || {}}
        onChange={handleTableChange}
      />
    </PageWrapper>
  );
};
