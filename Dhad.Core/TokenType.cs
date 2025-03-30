namespace Dhad.Core
{
    /// <summary>
    /// Defines the types of tokens recognized by the Dhad language lexer.
    /// </summary>
    public enum TokenType
    {
        // --- Single-character tokens ---
        LeftParen,      // (
        RightParen,     // )
        LeftBracket,    // [
        RightBracket,   // ]
        Comma,          // ,
        Dot,            // . (Potentially for future use, e.g., decimals or member access)
        Plus,           // +
        Minus,          // -
        Star,           // *
        Slash,          // /
        Percent,        // %
        Caret,          // ^ (For exponentiation)

        // --- One or two character tokens ---
        Equal,          // =
        NotEqual,       // <>
        Greater,        // >
        GreaterEqual,   // >=
        Less,           // <
        LessEqual,      // <=

        // --- Literals ---
        Identifier,     // Variable names, function names etc. (e.g., عدد, تربيع)
        String,         // String literals (e.g., "نص هنا")
        Number,         // Numeric literals (e.g., 123, 3.14)

        // --- Keywords ---
        // Declarations & I/O
        Keyword_Adkhel,      // أدخل
        Keyword_Masfoofa,    // مصفوفة
        Keyword_Thabit,      // ثابت
        Keyword_Mutaghayyer, // متغير
        Keyword_Azher,       // أظهر
        Keyword_Arjea,       // أرجع
        Keyword_Sater,       // سطر (Newline)
        Keyword_Masah,       // مسح (Clear screen)

        // Control Flow
        Keyword_Itha,        // إذا
        Keyword_FaInnaho,    // فإنه
        Keyword_WaIlla,      // وإلا
        Keyword_Nihaya_Itha, // نهاية إذا (Specific end for IF)

        Keyword_Min,         // من (Start of FOR loop)
        Keyword_Ila,         // إلى (End of FOR loop range)
        Keyword_Bekhotwa,    // بخطوة (Step in FOR loop)
        Keyword_Baynama,     // بينما (WHILE loop)
        Keyword_Karrer,      // كرر (End of loop body for FOR/WHILE)

        Keyword_Dalla,       // دالة (Function definition)
        Keyword_Nihaya_Dalla,// نهاية الدالة (Specific end for FUNCTION)

        // Operators & Logic
        Keyword_Wa,          // و (logical and)
        Keyword_Aw,          // أو (logical or)
        Keyword_Nafi,        // نفي (logical not)
        Keyword_Sah,         // صح (true literal)
        Keyword_Khata,       // خطأ (false literal)

        // Graphics
        Keyword_Nafitha,     // نافذة (Drawing window properties)
        Keyword_AlQalam,     // القلم (Drawing pen properties)
        Keyword_Irsem,       // ارسم (Draw command)
        Keyword_Noqta,       // نقطة (Point shape)
        Keyword_Khat,        // خط (Line shape)
        Keyword_Daera,       // دائرة (Circle shape)
        Keyword_Mostateel,   // مستطيل (Rectangle shape)
        Keyword_Nas,         // نص (Text drawing)
        Keyword_Lawnaho,     // لونه (Color property - Pen/Shape)
        Keyword_Ardoho,      // عرضه (Width property - Pen/Window)
        Keyword_Tooloha,     // طولها (Height property - Window)
        // Add specific color keywords if desired (e.g., Keyword_Ahmar for أحمر)

        // Math & Built-ins (Treating them as keywords simplifies parsing)
        Keyword_Ja,          // جا (Sin)
        Keyword_Jata,        // جتا (Cos)
        Keyword_Za,          // ظا (Tan)
        Keyword_Motlaq,      // مطلق (Abs)
        Keyword_Sahih,       // صحيح (Int/Floor)
        Keyword_Qarrab,      // قرب (Round)
        Keyword_Jathr,       // جذر (Sqrt)
        Keyword_Natej,       // ناتج (Eval string expression)
        Keyword_Ashwaey,     // عشوائي (Random)
        Keyword_Taa,         // ط (PI constant)
        Keyword_ToolAlnas,   // طول_النص (String length)
        Keyword_Alharf,      // الحرف (Character at index)
        Keyword_JuzAlnas,    // جزء_النص (Substring)
        Keyword_Alwaqt_W_Altareekh, // الوقت_و_التاريخ (DateTime Now)
        Keyword_Altareekh,   // التاريخ (Date Today)
        Keyword_Alwaqt,      // الوقت (Time Now)


        // --- End of File ---
        EOF
    }
}