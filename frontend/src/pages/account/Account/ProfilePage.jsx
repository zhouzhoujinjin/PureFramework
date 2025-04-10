import React, { useEffect, useState } from "react";
import { ImgCrop, PageWrapper } from "~/components";
import { GetPinYin } from "~/services/utils";
import {
  formItemLayout,
  formItemLayoutWithoutLabel,
} from "~/utils/formLayouts";
import { Form, Input, Upload, Button, Divider, message } from "antd";
import { useAuth } from "~/hooks";
import { token } from "~/utils/token";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { useDocumentTitle } from "~/hooks/useDocumentTitle";
import { UpdateProfile, UpdatePassword } from "./services/account";

const validateMessages = {
  required: "此项必填",
};
export const ProfilePage = () => {
  const {
    user: { userName, profiles },
    updateProfile,
  } = useAuth();
  const [form] = Form.useForm();
  const [data, setData] = useState(profiles);
  const { setFieldsValue } = form;
  useDocumentTitle("/profile", "个人设置");

  useEffect(() => {
    setFieldsValue({
      ...data,
      oldPassword: "",
      newPassword: "",
      confirmPassword: "",
    });
  }, [setFieldsValue, data]);

  const onPinYinChange = (e) => {
    e.preventDefault();
    var fullName = form.getFieldValue("surname") + e.target.value;
    GetPinYin(fullName, (data) => {
      form.setFieldsValue({
        pinyin: data,
      });
    });
  };
  const uploadButton = (
    <div>
      <FontAwesomeIcon icon="user" size="2x" />
      <div className="ant-upload-text">上传头像</div>
    </div>
  );
  const onAvatarChange = (file) => {
    if (file.file.response) {
      const {
        file: {
          status,
          response: { data },
        },
      } = file;
      if (status === "done" && data) {
        setData((p) => ({ ...p, avatar: data }));
      }
    }
  };

  const handleFinish = (values) => {
    const { oldPassword, newPassword, confirmPassword, ...rest } = values;
    rest.fullName = rest.surname + rest.givenName;
    let profileChanged = false;
    let passwordChanged = false;
    if (
      rest.surname !== profiles.surname ||
      rest.givenName !== profiles.givenName ||
      rest.pinyin !== profiles.pinyin ||
      rest.avatar !== profiles.avatar
    ) {
      profileChanged = true;
      UpdateProfile(rest, (data) => {
        updateProfile(data);
      });
    }
    if (oldPassword && newPassword && confirmPassword) {
      passwordChanged = true;
      if (oldPassword === newPassword) {
        message.info("新密码和当前密码相同");
      } else if (newPassword === confirmPassword) {
        UpdatePassword(oldPassword, newPassword);
      }
    }
    if (!profileChanged && !passwordChanged) {
      message.info("未修改资料");
    }
  };
  return (
    <PageWrapper title="个人设置">
      <Form
        form={form}
        onFinish={handleFinish}
        validateMessages={validateMessages}
      >
        <Form.Item label="用户名" {...formItemLayout}>
          <span className="ant-form-text">{userName}</span>
        </Form.Item>
        <Form.Item
          name="surname"
          label="姓"
          rules={[{ required: true }]}
          {...formItemLayout}
        >
          <Input />
        </Form.Item>
        <Form.Item
          name="givenName"
          label="名"
          rules={[{ required: true }]}
          {...formItemLayout}
        >
          <Input onBlur={onPinYinChange} />
        </Form.Item>
        <Form.Item name="pinyin" label="拼音" {...formItemLayout}>
          <Input placeholder="拼音" />
        </Form.Item>
        <Form.Item name="avatar" label="头像" {...formItemLayout}>
          <ImgCrop>
            <Upload
              name="avatar"
              listType="picture-card"
              className="avatar-uploader"
              showUploadList={false}
              action="/api/account/avatar"
              headers={{
                authorization: `Bearer ${token.get()}`,
              }}
              onChange={onAvatarChange}
            >
              {data.avatar ? (
                <img src={data.avatar} alt="avatar" style={{ width: "100%" }} />
              ) : (
                uploadButton
              )}
            </Upload>
          </ImgCrop>
        </Form.Item>
        <Divider>修改密码</Divider>
        <Form.Item name="oldPassword" label="当前密码" {...formItemLayout}>
          <Input
            type="password"
            placeholder="请输入当前密码，不修改请保持空白"
            autoComplete="new-password"
          />
        </Form.Item>
        <Form.Item
          name="newPassword"
          label="新密码"
          rules={[
            ({ getFieldValue }) => ({
              validator(rule, value) {
                if (getFieldValue("oldPassword")) {
                  if (value) {
                    if (value.length <= 6) {
                      return Promise.reject("密码必须大于 6 位");
                    }
                    var patten = /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)[^]{7,16}$/;
                    if (patten.test(value)) {
                      return Promise.resolve();
                    } else {
                      return Promise.reject(
                        "密码强度不足，必须包含大写字母，小写字母和数字"
                      );
                    }
                  } else {
                    return Promise.reject("必须填写密码");
                  }
                }
                return Promise.resolve();
              },
            }),
          ]}
          {...formItemLayout}
        >
          <Input
            type="password"
            placeholder="不修改请保持空白"
            autoComplete="new-password"
          />
        </Form.Item>
        <Form.Item
          name="confirmPassword"
          label="确认新密码"
          rules={[
            ({ getFieldValue }) => ({
              validator(rule, value) {
                if (getFieldValue("newPassword") === value) {
                  return Promise.resolve();
                }
                return Promise.reject("两次输入的密码不同");
              },
            }),
          ]}
          {...formItemLayout}
        >
          <Input
            type="password"
            placeholder="确认密码"
            autoComplete="new-password"
          />
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
