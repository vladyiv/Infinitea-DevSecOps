import { UserManager, WebStorageStateStore } from 'oidc-client-ts'

export const userManager = new UserManager({
  authority: 'http://localhost:8083/realms/infinitea',
  client_id: 'infinitea-frontend',

  redirect_uri: 'http://localhost:5173/',
  post_logout_redirect_uri: 'http://localhost:5173/',

  response_type: 'code',
  scope: 'openid profile email',

  userStore: new WebStorageStateStore({
    store: window.localStorage
  })
})
