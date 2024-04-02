fn main() {
    bindgen::Builder::default()
        .header("../../external/whisper.cpp/whisper.h")
        .generate().unwrap()
        .write_to_file("whisper.rs").unwrap();

    csbindgen::Builder::default()
        .input_bindgen_file("whisper.rs")            // read from bindgen generated code
        .rust_file_header("use super::whisper::*;")     // import bindgen generated modules(struct/method)
        .csharp_dll_name("whisper")
        .csharp_namespace("WhisperCppLib")
        .generate_to_file("whisper_ffi.rs", "../../src/WhisperCppLib/NativeMethods.g.cs")
        .unwrap();
}

