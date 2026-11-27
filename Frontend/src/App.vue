
<script setup>
import { ref, onMounted } from 'vue'

const products = ref([])
const loading = ref(true)
const error = ref('')

const API_URL = 'http://localhost:8084'

async function loadProducts() {
  try {
    const response = await fetch(`${API_URL}/api/products`)

    if (!response.ok) {
      throw new Error('Не удалось загрузить товары')
    }

    products.value = await response.json()
  } catch (err) {
    error.value = 'Не удалось загрузить каталог. Проверь подключение.'
  } finally {
    loading.value = false
  }
}

onMounted(loadProducts)
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
  TEA COLLECTION
  <span class="cart-count">Корзина · 0</span>
</div>
    </header>

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
      <a href="#about">О проекте</a>
    </footer>
  </div>
</template>

<style>
@import url('https://fonts.googleapis.com/css2?family=Manrope:wght@400;500;600;700;800&family=Playfair+Display:wght@500;600;700&display=swap');

:root {
  --background: #211C19;
  --surface: #302722;
  --surface-light: #514035;
  --text: #E9E0D5;
  --muted: #C2A98F;
  --accent: #82977D;
  --border: #493A30;
}

* {
  box-sizing: border-box;
}

html {
  scroll-behavior: smooth;
}

body {
  margin: 0;
  background: var(--background);
  color: var(--text);
  font-family: 'Manrope', sans-serif;
}

a {
  color: inherit;
  text-decoration: none;
}

.header {
  min-height: 82px;
  padding: 0 8%;
  display: flex;
  align-items: center;
  justify-content: space-between;
  border-bottom: 1px solid var(--border);
}

.logo {
  font-size: 29px;
  font-weight: 800;
  letter-spacing: -1.5px;
}

.logo span {
  color: var(--accent);
}

nav {
  display: flex;
  gap: 34px;
}

nav a, .header-label {
  color: var(--muted);
  font-size: 12px;
}

nav a:hover, .footer a:hover {
  color: var(--accent);
}

.header-label {
  letter-spacing: 2px;
}

main {
  max-width: 1200px;
  margin: 0 auto;
  padding: 0 32px;
}

.hero {
  min-height: 440px;
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 70px 0;
}

.hero-content {
  max-width: 570px;
}

.eyebrow {
  color: var(--accent);
  font-size: 11px;
  font-weight: 700;
  letter-spacing: 2.5px;
}

h1, h2 {
  font-family: 'Playfair Display', serif;
  font-weight: 500;
}

h1 {
  margin: 22px 0;
  font-size: clamp(42px, 6vw, 64px);
  line-height: 1.12;
}

.hero-description {
  max-width: 390px;
  color: var(--muted);
  line-height: 1.8;
  font-size: 14px;
}

.hero-button {
  display: inline-flex;
  align-items: center;
  gap: 25px;
  margin-top: 25px;
  padding: 15px 20px;
  border-radius: 4px;
  background: var(--accent);
  color: var(--background);
  font-size: 12px;
  font-weight: 700;
  transition: background 0.2s;
}

.hero-button:hover {
  background: #A0B29A;
}

.hero-decoration {
  width: 260px;
  height: 260px;
  position: relative;
  display: flex;
  align-items: center;
  justify-content: center;
}

.decoration-circle {
  position: absolute;
  width: 220px;
  height: 220px;
  border: 1px solid #65513F;
  border-radius: 50%;
}

.decoration-circle::before,
.decoration-circle::after {
  content: '';
  position: absolute;
  inset: 20px;
  border: 1px solid #65513F;
  border-radius: 50%;
}

.decoration-circle::after {
  inset: 45px;
  border-color: var(--accent);
}

.decoration-text {
  z-index: 1;
  color: var(--muted);
  font-size: 9px;
  letter-spacing: 3px;
}

.cart-count {
  margin-left: 24px;
  padding: 9px 13px;
  border: 1px solid var(--border);
  border-radius: 4px;
  color: var(--text);
  letter-spacing: 0;
}

.catalog {
  padding: 45px 0 80px;
  border-top: 1px solid var(--border);
}

.section-heading {
  display: flex;
  align-items: end;
  justify-content: space-between;
  margin-bottom: 30px;
}

.section-heading h2 {
  margin: 12px 0 0;
  font-size: 32px;
}

.product-count {
  color: var(--muted);
  font-size: 12px;
}

.products {
  display: grid;
  grid-template-columns: repeat(3, minmax(0, 1fr));
  gap: 22px;
}

.product-card {
  overflow: hidden;
  background: var(--surface);
  border: 1px solid var(--border);
  border-radius: 5px;
  transition: transform 0.25s, border-color 0.25s;
}

.product-card:hover {
  transform: translateY(-5px);
  border-color: var(--accent);
}

.product-image {
  aspect-ratio: 1 / 1;
  position: relative;
  display: flex;
  justify-content: center;
  align-items: center;
  background: #FFFFFF;
}

.product-image img {
  width: 100%;
  height: 100%;
  object-fit: contain;
  padding: 20px;
}

.image-placeholder {
  color: var(--muted);
  font-size: 76px;
  opacity: 0.7;
}

.image-number {
  position: absolute;
  top: 15px;
  left: 15px;
  color: var(--muted);
  font-size: 11px;
  letter-spacing: 1px;
}

.product-info {
  padding: 20px;
}

.product-info h3 {
  margin: 0 0 9px;
  font-size: 18px;
}

.product-info p {
  min-height: 40px;
  margin: 0;
  color: var(--muted);
  font-size: 12px;
  line-height: 1.7;
}

.product-bottom {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-top: 22px;
}

.price {
  color: var(--accent);
  font-size: 17px;
  font-weight: 700;
}

.product-arrow {
  color: var(--muted);
  font-size: 19px;
}

.message {
  padding: 35px 0;
  color: var(--muted);
  font-size: 14px;
}

.error {
  color: #D28B7E;
}

.retry-button {
  display: block;
  margin-top: 15px;
  padding: 10px 16px;
  border: 1px solid var(--accent);
  border-radius: 4px;
  background: transparent;
  color: var(--text);
  cursor: pointer;
}

.about {
  padding: 65px 30px;
  margin-bottom: 60px;
  background: var(--surface);
  border: 1px solid var(--border);
  text-align: center;
}

.about h2 {
  font-size: clamp(25px, 4vw, 38px);
  line-height: 1.4;
}

.footer {
  min-height: 90px;
  padding: 20px 8%;
  display: flex;
  align-items: center;
  justify-content: space-between;
  border-top: 1px solid var(--border);
}

.footer > span, .footer > a:last-child {
  color: var(--muted);
  font-size: 11px;
}

@media (max-width: 700px) {
  .header {
    padding: 18px 22px;
    flex-wrap: wrap;
    gap: 16px;
  }

  .header-label {
    display: none;
  }

  nav {
    gap: 18px;
  }

  main {
    padding: 0 20px;
  }

  .hero {
    min-height: 360px;
  }

  .hero-decoration {
    display: none;
  }

  .products {
    grid-template-columns: repeat(2, minmax(0, 1fr));
    gap: 12px;
  }

  .product-info {
    padding: 13px;
  }

  .product-info h3 {
    font-size: 15px;
  }

  .product-info p {
    font-size: 11px;
  }

  .footer {
    padding: 20px;
    flex-wrap: wrap;
    gap: 15px;
  }
}

@media (max-width: 420px) {
  .products {
    grid-template-columns: 1fr;
  }
}
</style>

