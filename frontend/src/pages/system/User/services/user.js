import request from '~/utils/request'
import { API } from '~/config'

export const GetUsers = async ({ page, size }, {query, status}, action) => {
  status = status ? `/${status}` : ''
  let url = `${API}/admin/users${status}`
  const data = await request.get(url, { params: { query, page, size } })
  action(data)
}

export const AddUser = async (user, action) => {
  const data = await request.post(`${API}/admin/users`, user)
  action(data)
}

export const UpdateUser = async (user, action) => {
  const data = await request.post(`${API}/admin/users/${user.userName}`, user)
  action(data)
}

export const LockUser = async (userName, action) => {
  const data = await request.post(`${API}/admin/users/${userName}/lock`)
  action(data)
}

export const ClearAccountCache = () => {
  sessionStorage.removeItem('CURRENT_PERMISSIONS')
  sessionStorage.removeItem('CURRENT_MENU')
}

export const GetUser = async (userName, action) => {
  const data = await request.get(`${API}/users/${userName}`)
  action(data)
}

export const SetDelete = async (userName, action) => {
  const data = await request.post(`${API}/admin/users/${userName}/delete`)
  action(data)
}

export const SetActive = async (userName, action) => {
  const data = await request.post(`${API}/users/${userName}/active`)
  action(data)
}

export const ResetPassword = async (userName, action) => {
  const data = await request.post(`${API}/admin/users/${userName}/resetPassword`)
  action(data)
}
