import "./styles/helveg.scss";
import App from "./components/App.svelte";

const app = new App({
    target: document.getElementById("app")!,
    hydrate: false
});

export default app;
