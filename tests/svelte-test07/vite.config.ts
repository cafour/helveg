import { defineConfig } from 'vite'
import { svelte } from '@sveltejs/vite-plugin-svelte'
import { viteSingleFile } from "vite-plugin-singlefile"
import { ssr } from "vite-plugin-ssr/plugin";

// https://vitejs.dev/config/
export default defineConfig({
  build: {
    modulePreload: false,
    rollupOptions: {
      output: {
        manualChunks: () => "app"
      }
    }
  },
  plugins: [
    svelte({
      compilerOptions: {
        hydratable: true
      }
    }),
    ssr({
      prerender: true
    }),
    viteSingleFile()
  ]
})
