import { createApp } from 'vue'
import { createPinia } from 'pinia'
import './globals.css'
import App from './App.vue'
import router from './router/main.ts'
import Buefy from 'buefy'
import 'buefy/dist/css/buefy.css'

createApp(App)
    .use(createPinia())
    .use(router)
    .use(Buefy)
    .mount('#app')
