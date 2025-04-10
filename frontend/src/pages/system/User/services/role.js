import request from '~/utils/request'
import { API } from '~/config'

export const GetRoles = async ({ page, size }, action) => {
  const data = await request.get(`${API}/admin/roles`, { params: { page, size } })
  action(data)
}

export const GetPermissions = async (action) => {
  const data = await request.get(`${API}/admin/permissions`)
  action(data)
}

export const GetRole = async (name, action) => {
  const data = await request.get(`${API}/admin/roles/${name}`)
  action(data)
}

export const AddRole = async (payload, action) => {
  const data = await request.post(`${API}/admin/roles`, payload)
  action(data)
}

export const UpdateRole = async ({ name, data }, action) => {
  const result = await request.put(`${API}/admin/roles/${name}`, data)
  action(result)
}

export const GetUsers = async (action) => {
  const data = await request.get(`${API}/users`, {
    params: {
      query: JSON.stringify({ visible: 'true' })
    }
  })
  action(data.data)
}

export const IsExistRole = async (roleName, action) => {
  const data = await request.get(`${API}/admin/roles/${roleName}/isexist`)
  action(data)
}

export const DeleteRole = async (roleName, action) => {
  const data = await request.delete(`${API}/admin/roles/${roleName}`)
  action(data)
}
