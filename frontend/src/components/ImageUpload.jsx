import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { message, Upload } from "antd";
import React, { useEffect, useState } from "react";
const getBase64 = (img, callback) => {
  const reader = new FileReader();
  reader.addEventListener("load", () => callback(reader.result));
  reader.readAsDataURL(img);
};
const beforeUpload = (file) => {
  const isJpgOrPng = file.type === "image/jpeg" || file.type === "image/png";
  if (!isJpgOrPng) {
    message.error("只支持JPG或PNG文件");
  }
  return isJpgOrPng;
};
export const ImageUpload = ({ action, value, onChange }) => {
  const [loading, setLoading] = useState(false);
  const [imageUrl, setImageUrl] = useState();

  useEffect(() => {
    setImageUrl(value)
  }, [value])

  const handleChange = (info) => {
    if (info.file.status === "uploading") {
      setLoading(true);
      return;
    }
    if (info.file.status === "done") {
      console.log(info.file.response);
      setImageUrl(info.file.response.data);
      onChange(info.file.response.data);
    }
  };
  const uploadButton = (
    <div>
      <FontAwesomeIcon icon={loading ? "refresh" : "plus"} size="lg" />
      <div
        style={{
          marginTop: 8,
        }}
      >
        上传
      </div>
    </div>
  );
  return (
    <Upload
      name="file"
      listType="picture-card"
      showUploadList={false}
      action={action}
      beforeUpload={beforeUpload}
      onChange={handleChange}
    >
      {imageUrl ? (
        <img
          src={imageUrl}
          style={{
            width: "100%",
          }}
        />
      ) : (
        uploadButton
      )}
    </Upload>
  );
};
