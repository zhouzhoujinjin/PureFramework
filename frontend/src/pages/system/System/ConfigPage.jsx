import React, { useEffect } from "react";
import { Form, Button, Input } from "antd";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { PageWrapper, ColorPicker } from "~/components";
import { SaveConfig } from "./services/config";
import { useGlobal, useDocumentTitle } from "~/hooks";
import {
  formItemLayout,
  formItemLayoutWithoutLabel,
} from "~/utils/formLayouts";

const validateMessages = {
  required: "必填要素",
};

export const ConfigPage = () => {
  const [form] = Form.useForm();
  const { setFieldsValue } = form;

  const { config, refresh } = useGlobal();

  useDocumentTitle("/system/config", "系统设置");

  useEffect(() => {
    setFieldsValue({
      ...config,
    });
  }, [setFieldsValue, config]);

  const onFinish = (values) => {
    const formData = values;
    SaveConfig(formData, (data) => {
      refresh();
    });
  };

  return (
    <PageWrapper title="系统设置" extras={null}>
      <Form
        name="configInfo"
        form={form}
        onFinish={onFinish}
        validateMessages={validateMessages}
        style={{ minHeight: 400 }}
      >
        <Form.Item label="站点标题" name="siteTitle" {...formItemLayout}>
          <Input />
        </Form.Item>
        <Form.Item
          label="看板前景色"
          name="foregroundColor"
          {...formItemLayout}
        >
          <ColorPicker />
        </Form.Item>
        <Form.Item
          label="看板背景色"
          name="backgroundColor"
          {...formItemLayout}
        >
          <ColorPicker />
        </Form.Item>
        <Form.Item {...formItemLayoutWithoutLabel}>
          <Button type="primary" htmlType="submit">
            <FontAwesomeIcon icon="save" /> 保存
          </Button>
        </Form.Item>
      </Form>
    </PageWrapper>
  );
};
