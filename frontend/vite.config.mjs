import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";
import path from "path";
import { antdDayjs } from "antd-dayjs-vite-plugin";
import { viteStaticCopy } from "vite-plugin-static-copy";
import antdIconAlias from "@slobber/antd-icons-fontawesome";

export default defineConfig(({ mode }) => {
  return {
    resolve: {
      alias: {
        "~": path.resolve(__dirname, "src"),
        ...antdIconAlias("fal"),
      },
    },
    build: {
      rollupOptions: {
        input: {
          main: path.resolve(__dirname, "index.html"),
          deliveryBoard: path.resolve(__dirname, "deliveryBoard/index.html"),
          satelliteBoard: path.resolve(__dirname, "satelliteBoard/index.html"),
        },
      },
    },
    server: {
      hmr: { overlay: true },
      proxy: {
        "/api": "http://localhost:7827",
        "/uploads": "http://localhost:7827",
        "/realtime": "http://localhost:7827",
      },
    },
    plugins: [
      antdDayjs(),
      react(),
      viteStaticCopy({
        targets: [
          {
            src: "node_modules/@fortawesome/fontawesome-pro/js/light.min.js",
            dest: "assets",
          },
          {
            src: "node_modules/@fortawesome/fontawesome-pro/js/solid.min.js",
            dest: "assets",
          },
        ],
      }),
    ],
    css: {
      preprocessorOptions: {
        less: {
          // 支持内联 JavaScript
          javascriptEnabled: true,
        },
      },
    },
  };
});
