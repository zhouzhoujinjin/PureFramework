import request from '~/utils/request'
import { API } from '~/config'

export const GetAccountMenu = async (action) => {
  const cached = sessionStorage.getItem('CURRENT_MENU')
  if (cached) {
    action(JSON.parse(cached))
  } else {
    const data = await request.get(`${API}/account/menu`)
    sessionStorage.setItem('CURRENT_MENU', JSON.stringify(data))
    action(data)
  }
}

export const ClearAccountCache = () => {
  sessionStorage.removeItem('permissions')
  sessionStorage.removeItem('profile')
  sessionStorage.removeItem('CURRENT_MENU')
  sessionStorage.removeItem('OPENED_TABS')
  sessionStorage.removeItem('ACTIVE_TAB')
}
