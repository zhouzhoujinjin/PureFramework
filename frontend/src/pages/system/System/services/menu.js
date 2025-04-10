import request from '~/utils/request'
import { API } from '~/config'

export const GetMenu = async (action) => {
  const data = await request.get(`${API}/admin/system/menu`)
  action(data)
}

export const UpdateMenu = async ({ tree }, action) => {
  const data = await request.post(`${API}/admin/system/menu`, tree)
  action && action(data)
}
