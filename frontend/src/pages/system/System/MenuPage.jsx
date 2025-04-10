import React, { useState, useEffect } from "react";
import {
  Collapse,
  List,
  Checkbox,
  Row,
  Col,
  Button,
  Tree,
  Card,
  Input,
  Radio,
  Form,
  Switch,
} from "antd";
import { PageWrapper } from "~/components/PageWrapper";
import { GetMenu, UpdateMenu } from "./services/menu";
import { paths } from "~/routes";
import "./style.less";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { useDocumentTitle } from "~/hooks/useDocumentTitle";
import { loop } from "~/utils/utils";

const { Panel } = Collapse;
const { Item } = List;

let menuMap = {};

const NodeForm = (props) => {
  const { value, routes, onChange, type, autoSave } = props;
  const [form] = Form.useForm();

  const otherRoutes = routes.filter((x) => x !== value.path);

  useEffect(() => {
    form.setFieldsValue({
      path: value.path,
      title: value.title,
      iconName: value.iconName,
      hidden: !!value.hidden,
      hideChildren: value.hideChildren,
    });
  }, [form, value]);

  useEffect(() => {
    if (autoSave) {
      const index = setInterval(() => {
        form.submit();
      }, 500);
      return () => clearInterval(index);
    }
  }, [form, autoSave]);

  const onFinish = (values) => {
    const newValue = {
      ...value,
      ...values,
      type,
      icon: values.iconName && (
        <FontAwesomeIcon icon={values.iconName} fixedWidth />
      ),
    };
    onChange(value.path, newValue);
  };

  const checkExisted = (rule, value) => {
    return new Promise((resolve, reject) => {
      if (!value) {
        reject("路径不能为空");
      } else if (otherRoutes.indexOf(value) >= 0) {
        reject("已存在相同的路径");
      } else if (type === "action" && value.indexOf("/") > -1) {
        reject("动作项需要使用“:”分割");
      } else if (type === "route" && value.indexOf("/") !== 0) {
        reject("路由项需要以“/”开始");
      } else {
        resolve();
      }
    });
  };
  return (
    <Form layout="vertical" form={form} onFinish={onFinish}>
      <Form.Item
        name="path"
        label={type === "route" ? "路径" : "动作"}
        rules={[
          {
            required: true,
            validator: checkExisted,
          },
        ]}
      >
        <Input />
      </Form.Item>
      <Form.Item name="title" label="标题">
        <Input />
      </Form.Item>
      <Form.Item name="iconName" label="图标">
        <Input />
      </Form.Item>
      <Form.Item name="hidden" valuePropName="checked" label="隐藏自身">
        <Switch />
      </Form.Item>
      <Form.Item name="hideChildren" valuePropName="checked" label="隐藏子项">
        <Switch />
      </Form.Item>
      <Form.Item>
        {!autoSave && (
          <Button type="primary" onClick={() => form.submit()}>
            保存
          </Button>
        )}
        {autoSave && (
          <Button
            type="danger"
            onClick={() => props.onRemove && props.onRemove(value.path)}
          >
            删除此节点
          </Button>
        )}
      </Form.Item>
    </Form>
  );
};

export const MenuPage = (props) => {
  const [menu, setMenu] = useState([]);
  const [selectedRoutes, setSelectedRoutes] = useState([]);
  const [selectedNode, setSelectedNode] = useState({});
  const [showCard, setShowCard] = useState(false);
  const [routes, setRoutes] = useState(paths);
  useDocumentTitle("/system/menu", "菜单设置");
  // 初始化菜单
  useEffect(() => {
    GetMenu((result) => {
      loop(result, (item, index, array, parent) => {
        item.icon = item.iconName ? (
          <FontAwesomeIcon icon={item.iconName} fixedWidth />
        ) : null;
        item.key = item.path;
        item.parent = parent;
        const p = paths.find((p) => p.path === item.path);
        if (p) p.disabled = true;
        menuMap[item.path] = item;
      });
      setMenu(result);
      setRoutes([...paths]);
    });
  }, []);

  ///选中树节点
  const onSelect = (key) => {
    if (key.length === 1) {
      setSelectedNode(menuMap[key]);
      setShowCard(true);
    } else {
      setShowCard(false);
    }
  };

  //拖拽树节点
  const onDrop = (info) => {
    const dropKey = info.node.key;
    const dragKey = info.dragNode.key;
    const dropPos = info.node.pos.split("-");
    const dropPosition =
      info.dropPosition - Number(dropPos[dropPos.length - 1]);
    const data = [...menu];

    // Find dragObject
    let dragObj = menuMap[dragKey];
    let dropObj = menuMap[dropKey];

    let arr = dragObj.parent ? dragObj.parent.children : data;
    arr.splice(
      arr.findIndex((x) => x === dragObj),
      1
    );

    if (!info.dropToGap) {
      // 扔在了节点上面
      dropObj.children = dropObj.children || [];
      dragObj.parent = dropObj;
      dropObj.children.push(dragObj);
    } else if (
      (info.node.children || []).length > 0 && // 包含子节点
      info.node.expanded && // 节点展开了（驻停一段时间，箭头朝下了）
      dropPosition === 1 // 放在了节点下面
    ) {
      // 设为节点下的一个子节点
      dropObj.children = dropObj.children || [];
      dragObj.parent = dropObj;
      dropObj.children.unshift(dragObj);
    } else {
      arr = dropObj.parent ? dropObj.parent.children : data;
      const index = arr.findIndex((x) => x === dropObj);
      dragObj.parent = dropObj.parent;
      if (dropPosition === -1) {
        // 节点上面，插到节点前面
        arr.splice(index, 0, dragObj);
      } else {
        // 节点下面，插到节点后面
        arr.splice(index + 1, 0, dragObj);
      }
    }

    setMenu(data);
  };

  //选中路径list复选框
  const onCheckboxChange = (values) => {
    setSelectedRoutes(values);
  };

  //从路径list插入到tree
  const onClickInsert = () => {
    selectedRoutes.forEach((r) => {
      const node = { ...r, key: r.path, parent: null };
      menu.push(node);
      menuMap[r.path] = node;
      r.disabled = true;
    });
    setMenu([...menu]);
    setRoutes([...routes]);
  };

  //从自定义菜单中插入到tree
  const onInsert = (_, values) => {
    values.key = values.path;
    menu.push(values);
    menuMap[values.path] = values;
    setMenu([...menu]);
  };

  //保存页面
  const onSubmit = () => {
    const tree = JSON.parse(
      JSON.stringify(menu, (k, v) =>
        k === "parent" || k === "icon" ? undefined : v
      )
    );
    UpdateMenu({ tree }, () => {
      setShowCard(false);
      sessionStorage.removeItem("CURRENT_MENU");
    });
  };

  //暂存节点
  const saveNode = (path, values) => {
    const old = menuMap[values.path];
    menuMap[values.path] = {
      ...old,
      ...values,
    };

    const arr = menuMap[path].parent ? menuMap[path].parent.children : menu;
    arr.splice(
      arr.findIndex((x) => x.path === path),
      1,
      menuMap[values.path]
    );

    if (path !== values.path) {
      menuMap[values.path].key = values.path;
      delete menuMap[path];
    }
    setSelectedNode(menuMap[values.path]);
    setMenu([...menu]);
  };

  //删除节点
  const removeNode = (path) => {
    routes.forEach((route, i) => {
      if (route.path === path) {
        routes[i].disabled = false;
      }
    });
    if (menuMap[path].children && menuMap[path].children.length > 0) {
      menuMap[path].children.forEach((m) => {
        m.parent = menuMap[path].parent;
      });
    }
    const arr = menuMap[path].parent ? menuMap[path].parent.children : menu;
    arr.splice(
      arr.findIndex((x) => x.path === path),
      1,
      ...(menuMap[path].children || [])
    );
    delete menuMap[path];
    setMenu([...menu]);
    setRoutes([...routes]);
    setShowCard(false);
  };

  return (
    <PageWrapper
      title="菜单设置"
      extras={<Button onClick={onSubmit}>保存菜单</Button>}
    >
      <Row>
        <Col span={4}>
          <Collapse accordion defaultActiveKey="1">
            <Panel header="按路径添加" key="1">
              <div className="pc-menu list-wrapper">
                <Checkbox.Group
                  style={{ width: "100%" }}
                  onChange={onCheckboxChange}
                >
                  <List>
                    {routes
                      .filter((r) => !r.disabled)
                      .map((route) => (
                        <Item key={route.path}>
                          <Checkbox value={route}>{route.title}</Checkbox>
                        </Item>
                      ))}
                  </List>
                </Checkbox.Group>
              </div>
              <div className="insert-btns">
                <Button onClick={onClickInsert}>插入菜单</Button>
              </div>
            </Panel>
            <Panel header="自定义添加" key="2">
              <NodeForm
                type="action"
                routes={Object.keys(menuMap)}
                value={{ path: "" }}
                onChange={onInsert}
              />
            </Panel>
          </Collapse>
        </Col>
        <Col span={6} push={1}>
          <Tree
            className="draggable-tree"
            showIcon
            draggable
            blockNode
            onDrop={onDrop}
            onSelect={onSelect}
            treeData={menu}
            icon={(nodeData) => (
              <FontAwesomeIcon
                fixedWidth
                icon={
                  nodeData.type === "route"
                    ? "location-dot"
                    : "star-exclamation"
                }
              />
            )}
          ></Tree>
        </Col>
        <Col span={12} push={2}>
          {showCard ? (
            <Card
              title={`${selectedNode.title} [${
                selectedNode.type === "route" ? "路由" : "动作"
              }]`}
            >
              <NodeForm
                type={selectedNode.type || "route"}
                hasChildren={selectedNode.children}
                value={{ ...selectedNode, icon: selectedNode.icon }}
                onChange={saveNode}
                onRemove={removeNode}
                autoSave={true}
                routes={Object.keys(menuMap)}
              />
            </Card>
          ) : (
            ""
          )}
        </Col>
      </Row>
    </PageWrapper>
  );
};
