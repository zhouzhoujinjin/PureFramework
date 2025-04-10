import request from '~/utils/request'
import { API } from '~/config'

export const LoginAccount = async ({ username, password, captcha, uuid }, action, error) => {
  try {
    const data = await request.post(`${API}/account/login`, { username, password, captcha, uuid })
    console.log(data)
    action(data)
  } catch (e) {
    error(e)
  }
}
