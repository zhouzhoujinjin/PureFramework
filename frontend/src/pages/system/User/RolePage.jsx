import React, { useState, useEffect } from "react";
import { Input, Form, Button, Select } from "antd";
import {
  GetRole,
  GetPermissions,
  GetUsers,
  UpdateRole,
  IsExistRole,
  AddRole,
} from "./services/role";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { PageWrapper } from "~/components/PageWrapper";
import { useParams } from "react-router-dom";
import { TreeTransfer } from "./components";
import { GetMenu } from "./services/menu";
import {
  formItemLayout,
  formItemLayoutWithoutLabel,
} from "~/utils/formLayouts";
import { useTabLayout, useDocumentTitle } from "~/hooks";
import { getFriendlyUserName, loop } from "~/utils/utils";

const FormItem = Form.Item;

export const RolePage = (props) => {
  const params = useParams();
  console.log(params)
  const [isEdit, setIsEdit] = useState(false);
  const [role, setRole] = useState({
    claims: [],
    name: params.id === "create" ? "" : params.id,
    title: "",
    users: [],
  });
  const [permissions, setPermissions] = useState([]);
  const [users, setUsers] = useState([]);
  const [form] = Form.useForm();

  const { refreshTab } = useTabLayout();

  const { setFieldsValue } = form;
  useDocumentTitle(
    role.name === "create"
      ? "添加角色"
      : `编辑 ${form.getFieldValue("name") || ""}`
  );

  useEffect(() => {
    setPermissions([]);
    GetPermissions((ps) => {
      const map = {};
      ps.forEach((p) => {
        if (!map.hasOwnProperty(p.group)) {
          map[p.group] = {
            key: p.group,
            title: p.group,
            children: [],
          };
        }
        map[p.group].children.push({
          key: p.value,
          title: p.name,
          value: p.value,
          group: `后端 - ${p.group}`,
        });
      });
      setPermissions((p) => [
        ...p,
        {
          key: "api",
          title: "后端",
          children: Object.values(map),
        },
      ]);
    });
    GetMenu((data) => {
      loop(data, (item, index, array, parent) => {
        item.value = item.path;
        item.key = item.path;
        item.type = item.type === "route" ? "路由" : "动作";
        item.group = parent ? `${parent.group} - ${parent.title}` : "菜单";
      });
      setPermissions((p) => [
        ...p,
        {
          key: "route",
          title: "菜单",
          children: data,
        },
      ]);
    });
  }, []);

  useEffect(() => {
    if (params.id) {
      GetUsers((us) => {
        const users = us
          .sort((x, y) => (x.profiles.PinYin < y.profiles.PinYin ? -1 : 1))
          .map((u) => ({
            label:  `${u.profiles.FullName || "佚名"}`,
            value: u.id,
          }));
        setUsers(users);
      });
    }
  }, [params.id]);

  useEffect(() => {
    if (params.id && params.id !== "create") {
      GetRole(params.id, (data) => {
        const lastFormData = {
          ...role,
          title: data.title,
          claims: data.claims,
          users: data.users.map((o) => o.id),
        };
        setFieldsValue(lastFormData);
        setRole(lastFormData);
        setIsEdit(true);
      });
    }
  }, [params.id]);

  const handleRoleSubmit = (values) => {
    const { claims, users } = values;
    const typeAndValues = claims.map((o) => {
      if (o.slice(0, 1) === "/") {
        return `route,${o}`;
      } else if (
        o.startsWith("GET") ||
        o.startsWith("PUT") ||
        o.startsWith("POST") ||
        o.startsWith("DELETE")
      ) {
        return `api,${o}`;
      } else {
        return `action,${o}`;
      }
    });
    const briefUsers = users.map((o) => ({ id: o }));

    if (params.id !== "create") {
      const { name, ...data } = values;
      UpdateRole(
        { name, data: { ...data, users: briefUsers, claims: typeAndValues } },
        (r) => {
          refreshTab("/roles");
        }
      );
    } else {
      AddRole({ ...values, users: briefUsers, claims: typeAndValues }, (r) => {
        refreshTab("/roles");
      });
    }
  };
  return (
    <PageWrapper title={`角色：${role.name}`}>
      <Form
        layout="horizontal"
        form={form}
        initialValues={role}
        onFinish={handleRoleSubmit}
      >
        <FormItem
          label="名称"
          name="name"
          rules={[
            ({ getFieldValue }) => ({
              validator(rule, value, cb) {
                return new Promise((resolve, reject) => {
                  if (!value) {
                    reject("必填要素");
                  } else {
                    params.id === "create"
                      ? IsExistRole(value, (data) =>
                          data ? reject("已存在同名角色") : resolve()
                        )
                      : resolve();
                  }
                });
              },
            }),
          ]}
          {...formItemLayout}
        >
          <Input autoComplete="off" disabled={isEdit} />
        </FormItem>
        <Form.Item
          label="标题"
          name="title"
          rules={[
            {
              require: true,
              message: "必填要素",
            },
          ]}
          {...formItemLayout}
        >
          <Input autoComplete="off" />
        </Form.Item>
        <FormItem
          label="权限"
          name="claims"
          {...formItemLayout}
          valuePropName="targetKeys"
        >
          <TreeTransfer
            dataSource={permissions}
            titles={["待选项", "已选项"]}
            filterOption={(inputValue, item) =>
              item.title.indexOf(inputValue) !== -1 ||
              item.tag.indexOf(inputValue) !== -1
            }
          />
        </FormItem>
        <FormItem label="用户" name="users" {...formItemLayout}>
          <Select
            mode="multiple"
            className="users-checkbox"
            options={users}
            onChange={(values) => {
              setRole({ ...role, users: values });
            }}
          />
        </FormItem>
        <Form.Item {...formItemLayoutWithoutLabel}>
          <Button type="primary" htmlType="submit">
            <FontAwesomeIcon icon="save" /> 保存
          </Button>
        </Form.Item>
      </Form>
    </PageWrapper>
  );
};
