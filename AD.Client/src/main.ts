import { createApp } from 'vue'
import { createPinia } from 'pinia'
import './globals.css'
import App from './App.vue'
import router from './router/main.ts'
import Buefy from 'buefy'
import 'buefy/dist/css/buefy.css'

import { library } from '@fortawesome/fontawesome-svg-core'
import '@fortawesome/fontawesome-svg-core/styles.css'
import { fas } from '@fortawesome/free-solid-svg-icons'
import { FontAwesomeIcon } from '@fortawesome/vue-fontawesome'

library.add(fas)

const app = createApp(App)

app.component('font-awesome-icon', FontAwesomeIcon)

app.use(createPinia())
    .use(router)
    .use(Buefy, {
        defaultIconComponent: 'font-awesome-icon',
        defaultIconPack: 'fas'
    })
    .mount('#app')
