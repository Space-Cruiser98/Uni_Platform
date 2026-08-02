/**
 * Run with: node login-test.mjs
 * Opens browser at http://localhost:5173, refreshes, and logs in with admin credentials.
 */
import { chromium } from 'playwright';

const APP_URL = 'http://localhost:5173';
const EMAIL = 'admin1@school.edu';
const PASSWORD = 'Admin1!';

async function run() {
  const browser = await chromium.launch({ headless: false });
  const context = await browser.newContext();
  const page = await context.newPage();

  const errors = [];
  page.on('console', msg => {
    const t = msg.text();
    if (t.includes('error') || t.includes('Error') || t.includes('401') || t.includes('fetch')) errors.push(t);
  });

  try {
    console.log('Opening app...');
    await page.goto(APP_URL, { waitUntil: 'networkidle', timeout: 15000 });
    await page.reload({ waitUntil: 'networkidle' });
    console.log('Page loaded, filling login form...');

    await page.getByPlaceholder('Email').fill(EMAIL);
    await page.getByPlaceholder('Password').fill(PASSWORD);

    const [response] = await Promise.all([
      page.waitForResponse(r => r.url().includes('/api/auth/login'), { timeout: 15000 }).catch(() => null),
      page.getByRole('button', { name: /Sign in/ }).click()
    ]);

    if (response) {
      const status = response.status();
      const body = await response.text().catch(() => '');
      console.log('Login API status:', status, 'body:', body.slice(0, 200));
    } else {
      console.log('No login API response seen (proxy or backend may be down)');
    }

    await page.waitForTimeout(2000);
    const path = new URL(page.url()).pathname;
    const errorEl = await page.locator('.error, [class*="error"]').first().textContent().catch(() => null);
    console.log('URL path:', path);
    console.log('Error on page:', errorEl || '(none)');
    if (errors.length) console.log('Console errors:', errors);

    if (path === '/') {
      console.log('SUCCESS: Logged in and on dashboard.');
    } else {
      console.log('Still on login page.');
    }
  } catch (e) {
    console.error('Error:', e.message);
  } finally {
    await browser.close();
  }
}

run();
