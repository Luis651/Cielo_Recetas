# Guía de Configuración de Base de Datos (Supabase)

Para que la aplicación funcione correctamente, debes ejecutar el script SQL en tu panel de Supabase.

## Pasos a seguir:

1.  Inicia sesión en [Supabase.com](https://supabase.com/).
2.  Entra en tu proyecto (**rajbdxggnaudubjxqbox**).
3.  En el menú lateral izquierdo, haz clic en **SQL Editor**.
4.  Haz clic en **New query** (o usa una existente).
5.  Copia y pega el contenido del archivo `supabase_schema.sql` que se encuentra en la carpeta del proyecto.
6.  Haz clic en el botón **Run** (Ejecutar).

### Script SQL (`supabase_schema.sql`):

```sql
-- Habilitar extensiones necesarias
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- Tabla de Recetas
CREATE TABLE IF NOT EXISTS public.recipes (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    title TEXT NOT NULL,
    description TEXT,
    image_url TEXT,
    category TEXT,
    difficulty TEXT,
    prep_time_minutes INTEGER,
    is_favorite BOOLEAN DEFAULT FALSE,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT timezone('utc'::text, now()) NOT NULL,
    user_id UUID REFERENCES auth.users(id) ON DELETE CASCADE
);

-- Tabla de Ingredientes
CREATE TABLE IF NOT EXISTS public.ingredients (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    recipe_id UUID REFERENCES public.recipes(id) ON DELETE CASCADE,
    name TEXT NOT NULL,
    amount TEXT,
    unit TEXT
);

-- Tabla de Pasos (Instrucciones)
CREATE TABLE IF NOT EXISTS public.recipe_steps (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    recipe_id UUID REFERENCES public.recipes(id) ON DELETE CASCADE,
    step_number INTEGER NOT NULL,
    instruction TEXT NOT NULL
);

-- Habilitar Row Level Security (Seguridad por filas)
ALTER TABLE public.recipes ENABLE ROW LEVEL SECURITY;
ALTER TABLE public.ingredients ENABLE ROW LEVEL SECURITY;
ALTER TABLE public.recipe_steps ENABLE ROW LEVEL SECURITY;

-- Políticas de seguridad (Opcional, para que el usuario solo vea sus recetas)
CREATE POLICY "Users can manage their own recipes" 
ON public.recipes FOR ALL 
USING (auth.uid() = user_id);

CREATE POLICY "Users can manage ingredients of their own recipes" 
ON public.ingredients FOR ALL 
USING (EXISTS (
    SELECT 1 FROM public.recipes 
    WHERE public.recipes.id = public.ingredients.recipe_id 
    AND public.recipes.user_id = auth.uid()
));

CREATE POLICY "Users can manage steps of their own recipes" 
ON public.recipe_steps FOR ALL 
USING (EXISTS (
    SELECT 1 FROM public.recipes 
    WHERE public.recipes.id = public.recipe_steps.recipe_id 
    AND public.recipes.user_id = auth.uid()
));
```

> [!IMPORTANT]
> Asegúrate de que el botón **Run** de un mensaje de éxito. Esto creará las tablas necesarias para que la app no falle al intentar leer o escribir datos.
