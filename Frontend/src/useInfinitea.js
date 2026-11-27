
import { ref, computed, onMounted } from 'vue'
import { userManager } from './auth'

export function useInfinitea() {
  const products = ref([])
  const loading = ref(true)
  const error = ref('')

  const currentUser = ref(null)
  const authLoading = ref(true)
  const authError = ref('')

  const cartItems = ref([])
  const cartLoading = ref(false)
  const cartError = ref('')
  const cartOpen = ref(false)
  
  const orderLoading = ref(false)
  const orderError = ref('')
  const lastOrder = ref(null)

  const API_URL = 'http://localhost:8084'

  const cartCount = computed(() =>
    cartItems.value.reduce((sum, item) => sum + item.quantity, 0)
  )

  const cartProducts = computed(() =>
    cartItems.value.map(item => ({
      ...item,
      product: products.value.find(
        product => product.id === item.productId
      )
    }))
  )

  const cartTotal = computed(() =>
    cartProducts.value.reduce((sum, item) => {
      return sum + (item.product?.price || 0) * item.quantity
    }, 0)
  )

  async function loadProducts() {
    try {
      const response = await fetch(`${API_URL}/api/products`)

      if (!response.ok) {
        throw new Error('Не удалось загрузить товары')
      }

      products.value = await response.json()
    } catch (err) {
      error.value = 'Не удалось загрузить каталог. Проверьте подключение.'
    } finally {
      loading.value = false
    }
  }

  async function loadCart() {
    if (!currentUser.value) return

    cartLoading.value = true
    cartError.value = ''

    try {
      const response = await fetch(
        `${API_URL}/api/cart/${currentUser.value.id}`
      )

      if (!response.ok) {
        throw new Error('Не удалось загрузить корзину')
      }

      cartItems.value = await response.json()
    } catch (err) {
      cartError.value = err.message
    } finally {
      cartLoading.value = false
    }
  }

  async function loadProfile(token) {
    const response = await fetch(`${API_URL}/api/users/me`, {
      headers: {
        Authorization: `Bearer ${token}`
      }
    })

    if (!response.ok) {
      throw new Error('Не удалось получить профиль пользователя')
    }

    currentUser.value = await response.json()
    await loadCart()
  }

  async function addToCart(product) {
    if (!currentUser.value) {
      login()
      return
    }

    cartError.value = ''

    try {
      const response = await fetch(
        `${API_URL}/api/cart/${currentUser.value.id}/items`,
        {
          method: 'POST',
          headers: {
            'Content-Type': 'application/json'
          },
          body: JSON.stringify({
            productId: product.id,
            quantity: 1
          })
        }
      )

      if (!response.ok) {
        throw new Error('Не удалось добавить товар')
      }

      await loadCart()
    } catch (err) {
      cartError.value = err.message
    }
  }

  async function updateCartItem(item, quantity) {
    if (quantity < 1) return

    cartError.value = ''

    try {
      const response = await fetch(
        `${API_URL}/api/cart/${currentUser.value.id}/items/${item.productId}`,
        {
          method: 'PUT',
          headers: {
            'Content-Type': 'application/json'
          },
          body: JSON.stringify({ quantity })
        }
      )

      if (!response.ok) {
        throw new Error('Не удалось изменить количество')
      }

      await loadCart()
    } catch (err) {
      cartError.value = err.message
    }
  }

  async function removeFromCart(productId) {
    cartError.value = ''

    try {
      const response = await fetch(
        `${API_URL}/api/cart/${currentUser.value.id}/items/${productId}`,
        {
          method: 'DELETE'
        }
      )

      if (!response.ok) {
        throw new Error('Не удалось удалить товар')
      }

      await loadCart()
    } catch (err) {
      cartError.value = err.message
    }
  }
  
  
async function checkout() {
  if (!currentUser.value || cartItems.value.length === 0) {
    return
  }

  orderLoading.value = true
  orderError.value = ''
  lastOrder.value = null

  try {
    const response = await fetch(
      `${API_URL}/api/orders/${currentUser.value.id}`,
      {
        method: 'POST'
      }
    )

    if (!response.ok) {
      const result = await response.json().catch(() => ({}))
      throw new Error(result.message || 'Не удалось оформить заказ')
    }

    const order = await response.json()

    lastOrder.value = order

    // Сервер уже очистил корзину после оформления.
    // Обновляем её содержимое на сайте.
    await loadCart()

    cartOpen.value = false
  } catch (err) {
    orderError.value = err.message
  } finally {
    orderLoading.value = false
  }
}


  async function initAuth() {
    try {
      const params = new URLSearchParams(window.location.search)

      if (params.has('code') && params.has('state')) {
        const user = await userManager.signinRedirectCallback()

        window.history.replaceState(
          {},
          document.title,
          window.location.pathname
        )

        if (user && !user.expired) {
          await loadProfile(user.access_token)
        }
      } else {
        const user = await userManager.getUser()

        if (user && !user.expired) {
          await loadProfile(user.access_token)
        }
      }
    } catch (err) {
      console.error('Ошибка авторизации:', err)
      authError.value = err.message
    } finally {
      authLoading.value = false
    }
  }

  function login() {
    userManager.signinRedirect()
  }

  async function logout() {
    await userManager.signoutRedirect()
  }

  onMounted(() => {
    loadProducts()
    initAuth()
  })

  return {
    products,
    loading,
    error,
    currentUser,
    authLoading,
    authError,
    cartItems,
    cartLoading,
    cartError,
    cartOpen,
    cartCount,
    cartProducts,
    cartTotal,
    cartItems,
    loadProducts,
    addToCart,
    updateCartItem,
    removeFromCart,
    checkout,
    login,
    logout,
    orderLoading,
    orderError,
    lastOrder
  }
}

