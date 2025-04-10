import React, { useState, useEffect } from "react"
import { Form, Input, Button, Upload } from "antd"
import { ImgCrop, PageWrapper } from "~/components"
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome"
import { useParams, useHistory } from "react-router-dom"
import { AddUser, GetUser, UpdateUser } from "./services/user"
import { IsExistUser, GetPinYin } from "~/services/utils"
import { token } from "~/utils/token"
import { useTabLayout } from "~/hooks/useTabLayout"
import {
  formItemLayout,
  formItemLayoutWithoutLabel,
} from "~/utils/formLayouts"
import { useDocumentTitle } from "~/hooks/useDocumentTitle"

export const UserPage = (props) => {
  const params = useParams()
  const [userId] = useState(params.id)
  const [form] = Form.useForm()
  const [imageUrl, setImageUrl] = useState(null)
  const history = useHistory()
  const [lock, setLock] = useState(false)
  const { refreshTab, replaceTab } = useTabLayout()
  const { setFieldsValue } = form

  useDocumentTitle(
    `/users/${userId}`,
    userId === "create"
      ? "添加用户"
      : `编辑 ${form.getFieldValue("userName") || ""}`
  )

  const validateMessages = {
    required: "必填要素",
  }

  useEffect(() => {
    if (userId !== "create") {
      GetUser(userId, (data) => {
        const {
          userName,
          profiles: { Surname, GivenName, Pinyin, avatar },
        } = data
        setFieldsValue({ userName, Surname, GivenName, Pinyin })
        setImageUrl(avatar)
        setLock(true)
      })
    }
  }, [userId, setFieldsValue])

  const onFinish = (values) => {

    const { userName, ...profiles } = values
    profiles.avatar = imageUrl
    profiles.fullName = `${profiles.Surname}${profiles.GivenName}`    
    let formData = {
      userName,
      profiles,
    }
    if (params.id === "create") {
      AddUser(formData, (data) => {
        history.push(`/users/${data.userName}`)
        replaceTab({
          oldKey: "/users/create",
          newKey: `/users/${data.userName}`,
          title: data.profiles.fullName,
        })
      })
    } else {
      UpdateUser(formData, (data) => { })
    }
    refreshTab("/users")
  }

  const onPinYinChange = (e) => {
    e.preventDefault()
    var fullName = form.getFieldValue("Surname") + e.target.value
    GetPinYin(fullName, (data) => {
      form.setFieldsValue({
        Pinyin: data,
      })
    })
  }

  const uploadButton = (
    <div>
      {<FontAwesomeIcon icon="lock-alt" />}
      <div className="ant-upload-text">上传头像</div>
    </div>
  )

  const onAvatarChange = (file) => {
    if (file.file.response) {
      const {
        file: {
          status,
          response: { data },
        },
      } = file
      if (status === "done" && data) {
        setImageUrl(data)
      }
    }
  }

  return (
    <PageWrapper
      title={
        userId === "create"
          ? "新建用户"
          : `编辑 ${form.getFieldValue("userName") || ""}`
      }
    >
      <Form
        name="userInfo"
        form={form}
        onFinish={onFinish}
        validateMessages={validateMessages}
      >
        <Form.Item
          name="userName"
          label="用户名"
          rules={[
            ({ getFieldValue }) => ({
              validator(rule, value, cb) {
                return new Promise((resolve, reject) => {
                  if (!value) {
                    reject("必填要素")
                  } else {
                    userId === "create"
                      ? IsExistUser(value, (data) =>
                        data ? reject("已存在同名用户") : resolve()
                      )
                      : resolve()
                  }
                })
              },
            }),
          ]}
          {...formItemLayout}
        >
          <Input placeholder="用户登录名" disabled={lock} />
        </Form.Item>
        <Form.Item
          name="Surname"
          label="姓"
          rules={[{ required: true }]}
          {...formItemLayout}
        >
          <Input />
        </Form.Item>
        <Form.Item
          name="GivenName"
          label="名"
          rules={[{ required: true }]}
          {...formItemLayout}
        >
          <Input onBlur={onPinYinChange} />
        </Form.Item>
        <Form.Item name="Pinyin" label="拼音" {...formItemLayout}>
          <Input placeholder="拼音" />
        </Form.Item>
        <Form.Item name="avatar" label="头像" {...formItemLayout}>
          <ImgCrop>
            <Upload
              name="avatar"
              listType="picture-card"
              className="avatar-uploader"
              showUploadList={false}
              action="/api/users/avatar"
              headers={{
                authorization: `Bearer ${token.get()}`,
              }}
              onChange={onAvatarChange}
            >
              {imageUrl ? (
                <img src={imageUrl} alt="avatar" style={{ width: "100%" }} />
              ) : (
                uploadButton
              )}
            </Upload>
          </ImgCrop>
        </Form.Item>
        <Form.Item {...formItemLayoutWithoutLabel}>
          <Button type="primary" htmlType="submit">
            <FontAwesomeIcon icon="save" /> 保存
          </Button>
        </Form.Item>
      </Form>
    </PageWrapper>
  )
}
