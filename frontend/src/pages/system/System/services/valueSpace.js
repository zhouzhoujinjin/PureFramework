import request from '~/utils/request'
import { API } from '~/config'

export const FetchValueSpace = async (name, action) => {
  const data = await request.get(`${API}/valueSpaces/${name}`)
  action(data)
}

export const GetValueSpaces = async ({ page, size }, action) => {
  const data = await request.get(`${API}/admin/valueSpaces`, { params: { page, size } })
  action(data)
}

export const UpdateValueSpace = async ([oldName, vs], action) => {
  const data = await request.put(`${API}/admin/valueSpaces/${oldName}`, vs)
  action(data)
}
