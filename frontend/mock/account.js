export default [
  {
    url: "/api/account/login",
    method: "post",
    response: ({ body }) => {
      return {
        code: 0,
        message: "登录成功",
        data: {
          accessToken:
            "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJ6aG91amluIiwianRpIjoiNDYyYjc5MTYtYWE3NS00ZDhlLTllYTEtZjk2ODIyYTQ3ZjU4IiwibmFtZWlkIjoiemhvdWppbiIsIklkIjoiMyIsIm5iZiI6MTY0NjIwMjEyNCwiZXhwIjoxNjQ2MjA5MzI0LCJpYXQiOjE2NDYyMDIxMjQsImlzcyI6InpoZW5neWVzb25na2UiLCJhdWQiOiJ6aGVuZ3llc29uZ2tlIn0.5yX_7IbpWvJyi2fL82h4LDh5hTKsm3GB8f-pyk6g0k8",
        },
      };
    },
  },
  {
    url: "/api/account/permissions",
    method: "get",
    response: ({ body }) => {
      return {
        code: 0,
        data: [],
      };
    },
  },
  {
    url: "/api/account/profile",
    method: "get",
    response: ({ body }) => {
      return {
        code: 0,
        data: {
          id: 1,
          userName: "admin",
          profiles: {
            surname: "管",
            givenName: "理员",
            pinyin: "guanliyuan",
            fullName: "管理员",
          },
        },
      };
    },
  },
  {
    url: "/api/account/menu",
    method: "get",
    response: () => ({
      code: 0,
      message: null,
      data: [
        {
          type: "route",
          path: "/index",
          title: "数据看板",
          iconName: "dashboard",
          hideChildren: false,
          hidden: false,
        },
        {
          type: "route",
          path: "/devices",
          title: "设备管理",
          iconName: "ethernet",
          hideChildren: true,
          hidden: false,
          children: [
            { type: "route", path: "/devices/:key", title: "设备表单" },
          ],
        },
        {
          type: "route",
          path: "/qrcode/index",
          title: "二维码管理",
          iconName: "qrcode",
          hideChildren: true,
          hidden: false,
          children: [
            {
              type: "route",
              path: "/qrcode/create",
              title: "新建二维码批次",
            },
            {
              type: "route",
              path: "/qrcode/:key",
              title: "查看二维码批次",
            },
          ],
        },
        {
          type: "route",
          path: "/settings/index",
          title: "配置管理",
          iconName: "scroll",
          hideChildren: false,
          hidden: false,
          children: [
            {
              type: "route",
              path: "/settings/templates",
              title: "配置模板",
              children: [
                {
                  type: "route",
                  path: "/settings/templates/:key",
                  title: "模板表单",
                },
              ],
              hideChildren: true,
              hidden: false,
            },
            {
              type: "route",
              path: "/settings/fields",
              title: "字段管理",
            },
          ],
        },
        {
          type: "route",
          path: "/users/index",
          title: "用户管理",
          iconName: "users",
          hideChildren: false,
          hidden: false,
          children: [
            {
              type: "route",
              path: "/users",
              title: "用户",
              hideChildren: true,
              hidden: false,
              children: [
                {
                  type: "route",
                  path: "/users/:id",
                  title: "用户详情",
                  hideChildren: false,
                  hidden: false,
                },
                {
                  type: "action",
                  path: "user:add",
                  title: "允许添加用户",
                  hideChildren: true,
                  hidden: false,
                },
              ],
            },
            {
              type: "route",
              path: "/roles",
              title: "角色",
              hideChildren: true,
              hidden: false,
              children: [
                {
                  type: "route",
                  path: "/roles/:id",
                  title: "角色详情",
                  hideChildren: false,
                  hidden: false,
                },
              ],
            },
            {
              type: "route",
              path: "/departments",
              title: "部门",
              hideChildren: false,
              hidden: false,
            },
          ],
        },
        {
          type: "route",
          path: "/tenants",
          title: "租户管理",
          iconName: "house-user",
          hideChildren: true,
          hidden: false,
          children: [
            { type: "route", path: "/tenants/:id", title: "租户表单" },
          ],
        },
        {
          type: "route",
          path: "/system",
          title: "系统管理",
          iconName: "cogs",
          hideChildren: false,
          hidden: false,
          children: [
            {
              type: "route",
              path: "/system/config",
              title: "系统设置",
              hideChildren: false,
              hidden: false,
            },
            {
              type: "route",
              path: "/system/menu",
              title: "菜单设置",
              hideChildren: false,
              hidden: false,
            },
            {
              type: "route",
              path: "/system/valueSpaces",
              title: "值空间",
              hideChildren: true,
              hidden: false,
              children: [
                {
                  type: "route",
                  path: "/system/valueSpaces/:name",
                  title: "值空间",
                  hideChildren: false,
                  hidden: false,
                },
              ],
            },
            {
              type: "route",
              path: "/system/frontendPermission",
              title: "前端权限",
              hideChildren: false,
              hidden: false,
            },
          ],
        },
      ],
    }),
  },
];
