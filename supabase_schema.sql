-- ================================================
-- DulceRecetario – Script SQL para Supabase
-- Ejecutar en: Supabase Dashboard → SQL Editor
-- ================================================

-- 1. Tabla de recetas
CREATE TABLE IF NOT EXISTS public.recipes (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id         TEXT,
    name            TEXT NOT NULL,
    description     TEXT,
    category        TEXT,
    difficulty      TEXT NOT NULL DEFAULT 'Fácil',
    prep_time_minutes INT NOT NULL DEFAULT 0,
    servings        INT NOT NULL DEFAULT 1,
    image_url       TEXT,
    is_favorite     BOOLEAN NOT NULL DEFAULT FALSE,
    created_at      TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at      TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- 2. Tabla de ingredientes
CREATE TABLE IF NOT EXISTS public.ingredients (
    id          UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    recipe_id   UUID NOT NULL REFERENCES public.recipes(id) ON DELETE CASCADE,
    name        TEXT NOT NULL,
    quantity    TEXT NOT NULL,
    unit        TEXT,
    order_index INT NOT NULL DEFAULT 0
);

-- 3. Tabla de pasos de preparación
CREATE TABLE IF NOT EXISTS public.recipe_steps (
    id          UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    recipe_id   UUID NOT NULL REFERENCES public.recipes(id) ON DELETE CASCADE,
    step_number INT NOT NULL,
    description TEXT NOT NULL
);

-- ── Índices de rendimiento ────────────────────────────────────────────────────
CREATE INDEX IF NOT EXISTS idx_recipes_user_id   ON public.recipes(user_id);
CREATE INDEX IF NOT EXISTS idx_ingredients_recipe ON public.ingredients(recipe_id);
CREATE INDEX IF NOT EXISTS idx_steps_recipe       ON public.recipe_steps(recipe_id);

-- ── RLS (Row Level Security) – opcional, para cuando agregues auth ────────────
-- ALTER TABLE public.recipes ENABLE ROW LEVEL SECURITY;
-- CREATE POLICY "Users can CRUD their own recipes" ON public.recipes
--     USING (auth.uid()::text = user_id)
--     WITH CHECK (auth.uid()::text = user_id);

-- ── Datos de ejemplo ──────────────────────────────────────────────────────────
INSERT INTO public.recipes (name, description, category, difficulty, prep_time_minutes, servings, is_favorite)
VALUES
    ('Tarta de maracuyá', 'Una tarta cremosa con sabor intenso a maracuyá', 'Tartas', 'Intermedia', 45, 8, TRUE),
    ('Galletas de avena y chocolate', 'Galletas crujientes por fuera, suaves por dentro', 'Galletas', 'Fácil', 30, 24, FALSE),
    ('Mousse de chocolate negro', 'Mousse aireado con chocolate 70% cacao', 'Mousses', 'Intermedia', 20, 6, TRUE);
