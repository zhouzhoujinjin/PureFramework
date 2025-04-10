import React, { useState, useEffect } from 'react'
import { Select } from 'antd'
import { GetUsers } from '../../services/utils'

const { Option } = Select
const UserSelect = ({ onChange, onInit, value, mode, onSelectedUsers }) => {
  const [users, setUsers] = useState([])
  const [selects, setSelects] = useState(mode === 'multiple' ? [] : undefined)

  useEffect(() => {
    GetUsers((data) => {
      setUsers(data)
    })
  }, [])

  useEffect(() => {
    if (users && users.length) {
      onInit && onInit(value && value.length > 0 ? users.filter((u) => value.indexOf(u.id) > -1) : [])
    }
  }, [users, value, onInit])

  useEffect(() => {
    if (value) {
      setSelects(mode === 'multiple' ? [...value] : value)
      if (onSelectedUsers) {
        let selectedUsers = value.length > 0 && users.filter((u) => value.indexOf(u.id) > -1)
        onSelectedUsers(selectedUsers)
      }
    }
  }, [value, mode])

  const handleChange = (v) => {
    if (onChange) {
      onChange(v)
    }
    if (onSelectedUsers) {
      let selectedUsers = v && v.length > 0 && users.filter((u) => v.indexOf(u.id) > -1)
      onSelectedUsers(selectedUsers)
    }
  }

  return (
    <Select
      showSearch
      filterOption={(input, option) => option.children.toLowerCase().indexOf(input.toLowerCase()) >= 0}
      style={{ width: 240 }}
      mode={mode}
      placeholder='请选择人员'
      value={selects}
      onChange={handleChange}>
      {mode === 'multiple' && (
        <Option key={0} value={0}>
          全选
        </Option>
      )}
      {users &&
        users.map((o) => (
          <Option key={o.id} value={o.id}>
            {o.profiles.fullName || o.userName}
          </Option>
        ))}
    </Select>
  )
}

export default UserSelect
