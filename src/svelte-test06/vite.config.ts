import { defineConfig } from 'vite';
import { sveltekit } from '@sveltejs/kit/vite';
import { viteSingleFile } from "vite-plugin-singlefile"

export default defineConfig({
	plugins: [sveltekit()],
	build: {
		modulePreload: false,
		rollupOptions: {
			output: {
				manualChunks: () => "bundle"
			}
		}
	}
});
