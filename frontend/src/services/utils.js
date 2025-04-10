import request from '~/utils/request'
import { API } from '~/config'

export const GetPinYin = async (chinese, action) => {
  const data = await request.get(`${API}/tools/pinyin/${chinese}`)
  action(data)
}

export const IsExistUser = async (userName, action) => {
  const data = await request.get(`${API}/admin/users/${userName}/isexist`)
  action(data)
}

export const GetUsers = async (action) => {
  const data = await request.get(`${API}/users`, { params: { page: 1, size: 9999 } })
  action(data.data)
}

