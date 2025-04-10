import React, { useState, useEffect } from "react";
import { FetchValueSpace, UpdateValueSpace } from "./services/valueSpace";
import { Input, Form, Button, Select } from "antd";
import { PageWrapper } from "~/components/PageWrapper";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { useParams } from "react-router-dom";
import {
  formItemLayout,
  formItemLayoutWithoutLabel,
} from "~/utils/formLayouts";
import { useTabLayout } from "~/hooks/useTabLayout";
import { useDocumentTitle } from "~/hooks/useDocumentTitle";

const InputGroup = Input.Group;
const FormItem = Form.Item;

const CodeInput = (props) => {
  const { value, onRemove, key, removable, isFirst } = props;
  const triggerChange = (changedValue) => {
    const { onChange, value } = props;
    if (onChange) {
      onChange({
        ...value,
        ...changedValue,
      });
    }
  };

  const handleCodeChange = (e) => {
    triggerChange({ code: e.target.value });
  };

  const handleLabelChange = (e) => {
    triggerChange({ label: e.target.value });
  };

  const handleRemove = (e) => {
    onRemove(key);
  };

  return (
    <div className={`code-label-input ${isFirst ? "first" : ""}`}>
      <InputGroup compact>
        <Input
          placeholder="代码"
          autoComplete="off"
          value={value.code}
          onChange={handleCodeChange}
        />
        <Input
          placeholder="标签"
          autoComplete="off"
          value={value.label}
          onChange={handleLabelChange}
        />
        {removable ? (
          <Button onClick={handleRemove}>
            <FontAwesomeIcon icon="minus" />
          </Button>
        ) : (
          <Button disabled>
            <FontAwesomeIcon icon="minus" />
          </Button>
        )}
      </InputGroup>
    </div>
  );
};

const RangeInput = (props) => {
  const { value, onRemove, key, removable, isFirst } = props;
  const triggerChange = (changedValue) => {
    const { onChange, value } = props;
    if (onChange) {
      onChange({
        ...value,
        ...changedValue,
      });
    }
  };

  const handleChange = (e) => {
    triggerChange(e.target.value);
  };

  const handleRemove = (e) => {
    onRemove(key);
  };

  return (
    <div className={`code-label-input ${isFirst ? "first" : ""}`}>
      <InputGroup compact>
        <Input
          placeholder="代码"
          autoComplete="off"
          value={value}
          onChange={handleChange}
        />
        {removable ? (
          <Button onClick={handleRemove}>
            <FontAwesomeIcon icon="minus" />
          </Button>
        ) : (
          <Button disabled>
            <FontAwesomeIcon icon="minus" />
          </Button>
        )}
      </InputGroup>
    </div>
  );
};

export const ValueSpacePage = (props) => {
  const params = useParams();
  const [valueSpaceName] = useState(params.name);
  const [form] = Form.useForm();
  const { setFieldsValue } = form;
  const [valueSpace, setValueSpace] = useState({
    name: params.name,
    conditions: [],
  });
  const { refreshTab } = useTabLayout();
  useDocumentTitle(
    `/valueSpaces/${valueSpaceName}`,
    valueSpaceName === "create"
      ? "添加值空间"
      : `编辑 ${form.getFieldValue("userName") || ""}`
  );
  const checkCodeLabel = (rule, value) => {
    return new Promise((resolve, reject) => {
      if (
        form.getFieldsValue().conditions.filter((x) => x.code === value.code)
          .length > 1
      ) {
        reject("和其他代码相同");
      } else if (!value.code || !value.label) {
        reject("代码和标签不能为空");
      } else {
        resolve();
      }
    });
  };

  const renderConditions = (conditions, vsType) => {
    switch (vsType) {
      case 1:
      case "Code":
        return (
          <Form.List name="conditions">
            {(fields, { add, remove }) => {
              return (
                <>
                  {fields.map((field, index) => (
                    <Form.Item
                      {...field}
                      {...(index === 0
                        ? formItemLayout
                        : formItemLayoutWithoutLabel)}
                      label={index === 0 ? "代码 / 标签" : null}
                      key={field.key}
                      validateTrigger={["onChange", "onBlur"]}
                      rules={[{ validator: checkCodeLabel }]}
                    >
                      <CodeInput
                        removable={conditions.length > 1}
                        isFirst={index === 0}
                        onRemove={() => remove(field.name)}
                      />
                    </Form.Item>
                  ))}
                  <FormItem {...formItemLayoutWithoutLabel}>
                    <Button
                      type="dashed"
                      onClick={() => {
                        add({ code: "", label: "" });
                      }}
                      block
                    >
                      <FontAwesomeIcon icon="plus" /> 添加选项
                    </Button>
                  </FormItem>
                </>
              );
            }}
          </Form.List>
        );
      case 2:
      case "Regex":
        return (
          <FormItem {...formItemLayout} label="正则表达式" name="conditions">
            <Input autoComplete="off" />
          </FormItem>
        );
      case 3:
      case "Range":
        return (
          <Form.List name="conditions">
            {(fields, { add, remove }) => {
              return (
                <>
                  {fields.map((field, index) => (
                    <Form.Item
                      {...field}
                      {...(index === 0
                        ? formItemLayout
                        : formItemLayoutWithoutLabel)}
                      label={index === 0 ? "分隔点" : null}
                      key={field.key}
                      validateTrigger={["onChange", "onBlur"]}
                      rules={[
                        {
                          required: true,
                          whitespace: true,
                          message: "请输入分隔点",
                        },
                      ]}
                    >
                      <RangeInput
                        removable={conditions.length > 1}
                        isFirst={index === 0}
                        onRemove={() => remove(field.name)}
                      />
                    </Form.Item>
                  ))}
                  <FormItem {...formItemLayoutWithoutLabel}>
                    <Button
                      type="dashed"
                      onClick={() => {
                        add({ code: "", label: "" });
                      }}
                      block
                    >
                      <FontAwesomeIcon icon="plus" /> 添加选项
                    </Button>
                  </FormItem>
                </>
              );
            }}
          </Form.List>
        );
      default:
        return null;
    }
  };

  const handleSubmit = (e) => {
    e.preventDefault();
    form
      .validateFields()
      .then((values) => {
        const oldName = valueSpace.name;
        valueSpace.name = values["name"];
        valueSpace.title = values["title"];
        switch (valueSpace.valueSpaceType) {
          case 1:
          case "Code":
            valueSpace.conditions = values.conditions.reduce((map, obj) => {
              map[obj.code] = obj.label;
              return map;
            }, {});
            break;
          case 2:
          case "Regex":
          case 3:
          case "Range":
            valueSpace.conditions = values.conditions;
            break;
          default:
        }

        UpdateValueSpace([oldName, valueSpace], (data) => {
          const { conditions, ...vs } = data;
          switch (vs.valueSpaceType) {
            case 1:
            case "Code":
              vs.conditions = Object.keys(conditions)
                .sort()
                .map((k) => ({ code: k, label: conditions[k] }));
              break;
            case 2:
            case 3:
            case "Range":
            case "Regex":
              vs.conditions = conditions;
              break;
            default:
          }
          setValueSpace(vs);
          refreshTab("/valueSpaces");
          setFieldsValue(vs);
        });
      })
      .catch((error) => {
        console.log(error);
      });
  };

  useEffect(() => {
    if (valueSpaceName !== "create") {
      FetchValueSpace(valueSpaceName, (data) => {
        const { conditions, ...vs } = data;
        switch (vs.valueSpaceType) {
          case 1:
          case "Code":
            vs.conditions = Object.keys(conditions)
              .sort()
              .map((k) => ({ code: k, label: conditions[k] }));
            break;
          case 2:
          case 3:
          case "Regex":
          case "Range":
            vs.conditions = conditions;
            break;
          default:
        }
        setValueSpace(vs);
        setFieldsValue(vs);
      });
    }
  }, [valueSpaceName, setFieldsValue]);

  return (
    <PageWrapper title={`值空间设置：${valueSpace.title || ""}`}>
      <Form
        layout="horizontal"
        form={form}
        name="valueSpace"
        onFinish={handleSubmit}
      >
        {valueSpaceName === "create" && (
          <FormItem label="类型" name="valueSpaceType" {...formItemLayout}>
            <Select
              onChange={(v) =>
                setValueSpace((f) => ({ ...f, valueSpaceType: v }))
              }
            >
              <Select.Option value="code">代码</Select.Option>
              <Select.Option value="range">范围</Select.Option>
              <Select.Option value="regex">正则表达式</Select.Option>
            </Select>
          </FormItem>
        )}
        <FormItem
          label="名称"
          name="name"
          rules={[
            {
              required: true,
              whitespace: true,
              message: "请填写名称",
            },
          ]}
          {...formItemLayout}
        >
          <Input disabled autoComplete="off" />
        </FormItem>
        <FormItem label="标题" name="title" {...formItemLayout}>
          <Input autoComplete="off" />
        </FormItem>
        {valueSpace &&
          renderConditions(valueSpace.conditions, valueSpace.valueSpaceType)}
        <FormItem {...formItemLayoutWithoutLabel}>
          <Button type="primary" htmlType="submit" onClick={handleSubmit}>
            保存
          </Button>
        </FormItem>
      </Form>
    </PageWrapper>
  );
};
