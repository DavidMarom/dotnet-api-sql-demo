CREATE OR REPLACE PROCEDURE upsert_contact(
    IN p_first_name VARCHAR,
    IN p_last_name VARCHAR,
    IN p_phone_number VARCHAR,
    INOUT p_id INT DEFAULT NULL
)
LANGUAGE plpgsql
AS $$
BEGIN
    IF p_id IS NOT NULL THEN
        UPDATE contacts
        SET first_name   = p_first_name,
            last_name    = p_last_name,
            phone_number = p_phone_number
        WHERE id = p_id;

        -- If no row was updated, fall through to insert
        IF NOT FOUND THEN
            p_id := NULL;
        END IF;
    END IF;

    IF p_id IS NULL THEN
        INSERT INTO contacts (first_name, last_name, phone_number)
        VALUES (p_first_name, p_last_name, p_phone_number)
        RETURNING id INTO p_id;
    END IF;
END;
$$;
