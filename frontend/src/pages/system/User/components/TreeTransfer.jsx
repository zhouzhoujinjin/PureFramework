import React from "react";
import { Badge, Tag, Transfer, Tree } from "antd";

const isChecked = (selectedKeys, eventKey) => {
  return selectedKeys.indexOf(eventKey) !== -1;
};

const generateTree = (treeNodes = [], checkedKeys = []) => {
  return treeNodes.map((node) => {
    const children = node.children && generateTree(node.children, checkedKeys);
    return {
      ...node,
      children,
      title: node.title + (node.value ? ` [${node.value}]` : ""),
      disabled: checkedKeys.includes(node.key),
      disableCheckbox: !node.group,
    };
  });
};

export const TreeTransfer = ({ dataSource, targetKeys, ...restProps }) => {
  const transferDataSource = [];
  function flatten(list = []) {
    list.forEach((item) => {
      transferDataSource.push(item);
      item.children && flatten(item.children);
    });
  }
  flatten(dataSource);
  const treeData = generateTree(dataSource, targetKeys);

  return (
    <Transfer
      {...restProps}
      targetKeys={targetKeys}
      dataSource={transferDataSource}
      className="tree-transfer"
      render={(item) =>
        item.title +
        (item.value ? ` [${item.value}]` : "") +
        (item.group ? ` [${item.group}]` : "")
      }
      showSelectAll={false}
    >
      {({ direction, onItemSelect, selectedKeys }) => {
        if (direction === "left") {
          const checkedKeys = [...selectedKeys, ...targetKeys];
          return (
            <Tree
              blockNode
              checkable
              checkStrictly
              defaultExpandAll
              checkedKeys={checkedKeys}
              titleRender={(nodeData) => (
                <>
                  {nodeData.title} {nodeData.type && <Tag>{nodeData.type}</Tag>}
                </>
              )}
              onCheck={(
                _,
                {
                  node: {
                    props: { eventKey },
                  },
                }
              ) => {
                onItemSelect(eventKey, !isChecked(checkedKeys, eventKey));
              }}
              onSelect={(
                _,
                {
                  node: {
                    disableCheckbox,
                    props: { eventKey },
                  },
                }
              ) => {
                if (!disableCheckbox) {
                  onItemSelect(eventKey, !isChecked(checkedKeys, eventKey));
                }
              }}
              treeData={treeData}
            ></Tree>
          );
        }
      }}
    </Transfer>
  );
};
