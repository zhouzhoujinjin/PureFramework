import axios from 'axios'
import { token } from './token'
import { API } from '../config'
import { message } from 'antd'

const request = axios.create({
  timeout: 60 * 60 * 1000
})

// 设置post请求头
request.defaults.headers.post['Content-Type'] = 'application/json'

// 添加请求拦截器
request.interceptors.request.use(
  (config) => {
    if (config.url.startsWith(API)) {
      const tokenStr = token.get()
      tokenStr && (config.headers.Authorization = `Bearer ${tokenStr}`)
    }
    return config
  },
  (error) => {
    return Promise.reject(error)
  }
)

// 添加响应拦截器
request.interceptors.response.use(
  (response) => {
    if (!response.config.url.startsWith(API)) {
      if (response.status === 200) {
        const body = response.data
        return Promise.resolve(body)
      }
      message.error('获取数据失败')
      return Promise.reject(response)
    } else {
      if (response.status === 200) {
        const body = response.data
        if (body.code === 0) {
          const { code, message: msg, ...result } = body
          if (msg) {
            message.success(msg)
          }
          // 如果结果除了 data 还有其他内容
          if (Object.keys(result).length === 1 && result.hasOwnProperty('data')) {
            return Promise.resolve(result.data)
          } else if(Object.keys(result).length > 1) {
            return Promise.resolve(result)
          } else {
            return Promise.resolve(undefined)
          }
        } else {
          body.message && message.error(body.message)
        }
      } else {
        return Promise.reject(response)
      }
    }
  },
  (error) => {
    // 相应错误处理
    // 比如： token 过期， 无权限访问， 路径不存在， 服务器问题等
    if (window.location.pathname.startsWith('/4')) {
      return
    }
    switch (error.response.status) {
      case 401:
        break
      case 403:
        message.error(`没有权限 [${error.config.url}]`)
        //window.location.href = '/403'
        break
      case 404:
        message.error(`找不到网址 [${error.config.url}]`)
        //window.location.href = '/404'
        break
      case 500:
        break
      case 504:
        message.error('连接超时')
        break
      default:
        message.error(error.response.data.message)
        console.log('其他错误信息')
    }
    return Promise.reject(error)
  }
)

export default request
