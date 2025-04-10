import React from 'react'
import { Result, Button } from 'antd'
import { useHistory } from 'react-router-dom'

export const View403 = () => {
  const history =  useHistory()
  return (
    <Result
      status='403'
      title='403'
      subTitle='没有权限访问此内容'
      extra={
        <Button type='primary' onClick={() => history.push('/')}>
          返回首页
        </Button>
      }
    />
  )
}
