-- ================================================
-- DulceRecetario – Script de Migración a Auth
-- Ejecutar en: Supabase Dashboard → SQL Editor
-- ================================================

-- 0. Limpiar datos de prueba existentes
DELETE FROM public.recipes;

-- 1. Migrar user_id de TEXT a UUID (para coincidir con auth.uid())
ALTER TABLE public.recipes
    ALTER COLUMN user_id TYPE UUID USING user_id::uuid;

-- 2. Hacer user_id NOT NULL y referenciar auth.users
ALTER TABLE public.recipes
    ALTER COLUMN user_id SET NOT NULL,
    ADD CONSTRAINT fk_recipes_user
        FOREIGN KEY (user_id) REFERENCES auth.users(id) ON DELETE CASCADE;

-- 3. Activar Row Level Security (RLS)
ALTER TABLE public.recipes ENABLE ROW LEVEL SECURITY;

-- 4. Política: solo el dueño puede leer sus recetas
CREATE POLICY "Leer propias recetas"
    ON public.recipes
    FOR SELECT
    USING (auth.uid() = user_id);

-- 5. Política: solo el dueño puede insertar (y el user_id se valida)
CREATE POLICY "Crear propias recetas"
    ON public.recipes
    FOR INSERT
    WITH CHECK (auth.uid() = user_id);

-- 6. Política: solo el dueño puede actualizar sus recetas
CREATE POLICY "Editar propias recetas"
    ON public.recipes
    FOR UPDATE
    USING (auth.uid() = user_id)
    WITH CHECK (auth.uid() = user_id);

-- 7. Política: solo el dueño puede eliminar sus recetas
CREATE POLICY "Eliminar propias recetas"
    ON public.recipes
    FOR DELETE
    USING (auth.uid() = user_id);

-- 8. Índice actualizado para el nuevo tipo UUID
DROP INDEX IF EXISTS idx_recipes_user_id;
CREATE INDEX idx_recipes_user_id ON public.recipes(user_id);

-- ================================================
-- Verificación (opcional, ejecutar por separado)
-- ================================================
-- SELECT tablename, rowsecurity FROM pg_tables WHERE tablename = 'recipes';
-- SELECT * FROM pg_policies WHERE tablename = 'recipes';
