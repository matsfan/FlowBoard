import { expect, afterEach, vi } from 'vitest';
// Attach expect to global for libraries expecting jest-style global
// @ts-ignore
globalThis.expect = expect;
import '@testing-library/jest-dom';
import { cleanup } from '@testing-library/react';

// Extend expect with jest-dom matchers
// (jest-dom auto extends global expect when using jest, but in vitest we ensure import order)

afterEach(() => {
	cleanup();
});
