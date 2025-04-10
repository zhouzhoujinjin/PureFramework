export default [
  {
    url: '/api/users',
    method: 'post',
    response: ({ body }) => {
      return {
        code: 0,
        msg: '成功添加用户',
        data: null,
      }
    },
  },
]