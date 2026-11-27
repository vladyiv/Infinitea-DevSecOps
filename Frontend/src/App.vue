<script setup>
import { useInfinitea } from './useInfinitea'

const {
  products,
  loading,
  error,
  currentUser,
  authLoading,
  authError,
  cartOpen,
  cartCount,
  cartProducts,
  cartLoading,
  cartError,
  cartTotal,
  cartItems,
  addToCart,
  updateCartItem,
  removeFromCart,
  login,
  logout,
  orderLoading,
  orderError,
  lastOrder,
  checkout
} = useInfinitea()
</script>

<template>
  <div class="page">
    <header class="header">
      <a href="#" class="logo">infinitea<span>.</span></a>

      <nav>
        <a href="#catalog">Каталог</a>
        <a href="#about">О нас</a>
      </nav>

      
<div class="header-label">
  <span>TEA COLLECTION</span>

  <button
  class="cart-count"
  @click="cartOpen = !cartOpen"
>
  Корзина · {{ cartCount }}
</button>

  <span v-if="authLoading" class="auth-status">
    Проверяем вход...
  </span>

  <template v-else-if="currentUser">
    <span class="user-label">
      {{ currentUser.username }}
    </span>

    <button class="auth-button" @click="logout">
      Выйти
    </button>
  </template>

  <button
    v-else
    class="auth-button"
    @click="login"
  >
    Войти
  </button>
</div>

    </header>
    
    
    <div v-if="authError" class="auth-error">
  {{ authError }}
</div>

<section v-if="cartOpen" class="cart-panel">
  <div class="cart-heading">
    <h2>Ваша корзина</h2>
    <button class="close-cart" @click="cartOpen = false">
      ✕
    </button>
  </div>

  <p v-if="!currentUser" class="message">
    Войдите, чтобы пользоваться корзиной.
  </p>

  <p v-else-if="cartLoading" class="message">
    Загружаем корзину...
  </p>

  <p v-else-if="cartError" class="message error">
    {{ cartError }}
  </p>

  <p
    v-else-if="cartProducts.length === 0"
    class="message"
  >
    Корзина пока пуста.
  </p>

  <div v-else>
    <div
      v-for="item in cartProducts"
      :key="item.id"
      class="cart-item"
    >
      <div class="cart-item-info">
        <strong>{{ item.product?.name }}</strong>
        <span>
          {{ item.product?.price }} ₽ × {{ item.quantity }}
        </span>
      </div>

      <div class="cart-controls">
        <button
          @click="updateCartItem(item, item.quantity - 1)"
          :disabled="item.quantity <= 1"
        >
          −
        </button>

        <span>{{ item.quantity }}</span>

        <button
          @click="updateCartItem(item, item.quantity + 1)"
        >
          +
        </button>

        <button
          class="remove-button"
          @click="removeFromCart(item.productId)"
        >
          Удалить
        </button>
      </div>
    </div>

    <div class="cart-total">
      <span>Итого</span>
      <strong>{{ cartTotal }} ₽</strong>
    </div>
  </div>
  
  
<div class="checkout-section">
  <p v-if="orderError" class="checkout-error">
    {{ orderError }}
  </p>

  <button
    class="checkout-button"
    @click="checkout"
    :disabled="orderLoading || cartLoading || cartItems.length === 0"
  >
    {{ orderLoading ? 'Оформляем заказ...' : 'Оформить заказ' }}
  </button>
</div>

</section>


<section v-if="lastOrder" class="order-success">
  <p class="eyebrow">ORDER CONFIRMED</p>

  <h2>Спасибо за заказ!</h2>

  <p class="order-number">
    Номер заказа: <strong>#{{ lastOrder.id }}</strong>
  </p>

  <p class="order-date">
    {{ new Date(lastOrder.createdAt).toLocaleString('ru-RU') }}
  </p>

  <div class="order-items">
    <div
      v-for="item in lastOrder.items"
      :key="item.id"
      class="order-item"
    >
      <span>
        {{ products.find(p => p.id === item.productId)?.name || `Товар #${item.productId}` }}
        × {{ item.quantity }}
      </span>

      <strong>
        {{ item.unitPrice * item.quantity }} ₽
      </strong>
    </div>
  </div>

  <button
    class="checkout-button"
    @click="lastOrder = null"
  >
    Продолжить покупки
  </button>
</section>



    <main>
      <section class="hero">
        <div class="hero-content">
          <p class="eyebrow">ИСКУССТВО ЧАЕПИТИЯ</p>

          <h1>Найди свой<br />идеальный чай</h1>

          <p class="hero-description">
            Открой для себя любимые вкусы
            и наслаждайся каждым моментом.
          </p>

          <a href="#catalog" class="hero-button">
            Исследовать коллекцию
            <span>↗</span>
          </a>
        </div>

        <div class="hero-decoration">
          <span class="decoration-circle"></span>
          <span class="decoration-text">A MOMENT FOR YOURSELF</span>
        </div>
      </section>

      <section id="catalog" class="catalog">
        <div class="section-heading">
          <div>
            <p class="eyebrow">ВЫБЕРИ СВОЙ ВКУС</p>
            <h2>Наша коллекция</h2>
          </div>

          <span class="product-count">
            {{ products.length }} товара
          </span>
        </div>

        <div v-if="loading" class="message">
          Завариваем каталог...
        </div>

        <div v-else-if="error" class="message error">
          {{ error }}
          <button class="retry-button" @click="loadProducts">
            Попробовать снова
          </button>
        </div>

        <div v-else-if="products.length" class="products">
          <article
            v-for="(product, index) in products"
            :key="product.id"
            class="product-card"
          >
            <div class="product-image">
              <img
  :src="[
    '/images/banana_split.webp',
    '/images/relax_1.png',
    '/images/jasmine_dream.webp'
  ][index]"
  :alt="product.name"
/>
              <span class="image-number">
                0{{ index + 1 }}
              </span>
            </div>

            <div class="product-info">
              <h3>{{ product.name }}</h3>
              <p>{{ product.description }}</p>

              <div class="product-bottom">
                <span class="price">{{ product.price }} ₽</span>
                <span class="product-arrow">↗</span>
              </div>
              
              <button
  class="add-cart-button"
  @click="addToCart(product)"
>
  В корзину
</button>
              
            </div>
          </article>
        </div>

        <div v-else class="message">
          Пока товаров нет.
        </div>
      </section>

      <section id="about" class="about">
        <p class="eyebrow">INFINITEA PHILOSOPHY</p>
        <h2>Иногда всё, что нужно —<br />это чашка хорошего чая.</h2>
      </section>
    </main>

    <footer class="footer">
      <a href="#" class="logo">infinitea<span>.</span></a>
      <span>Учебный проект · 2026</span>
      <a href="https://github.com/vladyiv/Infinitea-DevSecOps">О проекте</a>
    </footer>
  </div>
</template>


