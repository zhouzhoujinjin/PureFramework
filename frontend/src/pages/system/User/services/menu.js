import request from '~/utils/request'
import { API } from '~/config'

export const GetMenu = async (action) => {
  const data = await request.get(`${API}/admin/system/menu`)
  action(data)
}
