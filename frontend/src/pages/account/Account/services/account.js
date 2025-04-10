import request from '~/utils/request'
import { API } from '~/config'

export const UpdateProfile = async (profile, action) => {
  const data = await request.post(`${API}/account/profile`, profile)
  action && action(data)
}

export const UpdatePassword = async (oldPassword, newPassword) => {
  await request.post(`${API}/account/password`, { oldPassword, newPassword })
}
