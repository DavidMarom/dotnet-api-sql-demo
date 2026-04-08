CREATE OR REPLACE FUNCTION search_contacts(query TEXT)
RETURNS TABLE (
    id INT,
    first_name VARCHAR,
    last_name VARCHAR,
    phone_number VARCHAR
)
LANGUAGE sql
STABLE
AS $$
    SELECT id, first_name, last_name, phone_number
    FROM contacts
    WHERE
        first_name ILIKE '%' || query || '%'
        OR last_name ILIKE '%' || query || '%'
        OR (first_name || ' ' || last_name) ILIKE '%' || query || '%'
    ORDER BY last_name, first_name;
$$;
